using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.DTOs;
using System.Security.Claims;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using System.IdentityModel.Tokens.Jwt;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            ITokenBlacklistService tokenBlacklistService,
            IPasswordResetService passwordResetService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _tokenBlacklistService = tokenBlacklistService;
            _passwordResetService = passwordResetService;
        }

        /// <summary>
        /// User login with JWT token generation
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid input data"
                    });
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || !user.IsActive)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    try
                    {
                        // Update last login date
                        user.LastLoginDate = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);

                        // Get user roles
                        var roles = await _userManager.GetRolesAsync(user);

                        // Generate JWT tokens
                        var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
                        var refreshToken = _jwtService.GenerateRefreshToken(user.Id); // Pass userId

                        // Calculate expiry time
                        var expireMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRE_MINUTES") ?? "60");
                        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

                        var response = new LoginResponseDto
                        {
                            Success = true,
                            Message = "Login successful",
                            User = new UserProfileDto
                            {
                                Id = user.Id,
                                FullName = user.FullName,
                                Email = user.Email!,
                                EmployeeId = user.EmployeeId,
                                Department = user.Department?.Name,
                                Position = user.Position,
                                PhoneNumber = user.PhoneNumber,
                                Roles = roles.ToList(),
                                IsActive = user.IsActive,
                                LastLoginDate = user.LastLoginDate
                            },
                            AccessToken = accessToken,
                            RefreshToken = refreshToken,
                            ExpiresAt = expiresAt
                        };

                        return Ok(response);
                    }
                    catch (Exception tokenEx)
                    {
                        return StatusCode(500, new LoginResponseDto
                        {
                            Success = false,
                            Message = $"Login successful but token generation failed: {tokenEx.Message}"
                        });
                    }
                }

                if (result.IsLockedOut)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Account is locked due to multiple failed login attempts"
                    });
                }

                if (result.IsNotAllowed)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Account is not allowed to sign in"
                    });
                }

                return BadRequest(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = $"An error occurred during login: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Change password for current user
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponseDto.ErrorResult("Invalid input data", errors));
                }

                // Additional validation for password confirmation
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(ApiResponseDto.ErrorResult("New password and confirm password do not match"));
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponseDto.ErrorResult("User not found"));
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(ApiResponseDto.ErrorResult("User not found"));
                }

                // Verify current password
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    return BadRequest(ApiResponseDto.ErrorResult("Current password is incorrect"));
                }

                // Check if new password is same as current password
                if (request.CurrentPassword == request.NewPassword)
                {
                    return BadRequest(ApiResponseDto.ErrorResult("New password must be different from current password"));
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(ApiResponseDto.SuccessResult(null, "Password changed successfully"));
                }

                var changePasswordErrors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponseDto.ErrorResult("Failed to change password", changePasswordErrors));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDto.ErrorResult($"Error changing password: {ex.Message}"));
            }
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto>> Logout()
        {
            try
            {
                var jwtId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                // Check if token is already blacklisted
                if (!string.IsNullOrEmpty(jwtId) && await _tokenBlacklistService.IsTokenBlacklistedAsync(jwtId))
                {
                    return BadRequest(ApiResponseDto.ErrorResult("User already logged out"));
                }


                // Blacklist the current token
                if (!string.IsNullOrEmpty(jwtId))
                {
                    var expiry = DateTime.UtcNow.AddDays(7); // Token expiry time
                    await _tokenBlacklistService.BlacklistTokenAsync(jwtId, expiry);
                }

                await _signInManager.SignOutAsync();

                return Ok(ApiResponseDto.SuccessResult(null, "Logout successful"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDto.ErrorResult($"Error during logout: {ex.Message}"));
            }
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid || string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    });
                }

                // Validate refresh token using JWT service
                var tokenResponse = await _jwtService.RefreshTokenAsync(request.RefreshToken);

                if (tokenResponse == null || !tokenResponse.Success)
                {
                    return Unauthorized(new LoginResponseDto
                    {
                        Success = false,
                        Message = tokenResponse?.Message ?? "Invalid or expired refresh token"
                    });
                }

                // Get user information for response
                var user = await _userManager.FindByIdAsync(tokenResponse.UserId!);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new LoginResponseDto
                    {
                        Success = false,
                        Message = "User not found or inactive"
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);

                var response = new LoginResponseDto
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email!,
                        EmployeeId = user.EmployeeId,
                        Department = user.Department?.Name,
                        Position = user.Position,
                        PhoneNumber = user.PhoneNumber,
                        Roles = roles.ToList(),
                        IsActive = user.IsActive,
                        LastLoginDate = user.LastLoginDate
                    },
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    ExpiresAt = tokenResponse.ExpiresAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = $"Error refreshing token: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Send password reset OTP to user's email
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponseDto.ErrorResult("Invalid input data", errors));
                }

                var result = await _passwordResetService.SendPasswordResetOtpAsync(request.Email);

                if (result)
                {
                    return Ok(ApiResponseDto.SuccessResult(null, "Password reset OTP has been sent to your email"));
                }

                // Don't reveal if email exists or not for security
                return Ok(ApiResponseDto.SuccessResult(null, "If the email exists in our system, you will receive a password reset OTP"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDto.ErrorResult($"Error processing forgot password request: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reset password using OTP and generate new random password
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<PasswordResetResponseDto>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new PasswordResetResponseDto
                    {
                        Success = false,
                        Message = $"Invalid input data: {string.Join(", ", errors)}"
                    });
                }

                var (success, newPassword) = await _passwordResetService.ResetPasswordWithOtpAsync(request.Email, request.Otp);

                if (success)
                {
                    return Ok(new PasswordResetResponseDto
                    {
                        Success = true,
                        Message = "Password has been reset successfully. A new password has been sent to your email.",
                        NewPassword = newPassword // You can remove this if you don't want to return password in response
                    });
                }

                return BadRequest(new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "Invalid or expired OTP. Please try again."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new PasswordResetResponseDto
                {
                    Success = false,
                    Message = $"Error resetting password: {ex.Message}"
                });
            }
        }
    }
}