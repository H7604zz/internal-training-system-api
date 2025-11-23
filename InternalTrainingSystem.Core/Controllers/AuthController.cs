using DocumentFormat.OpenXml.Wordprocessing;
using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Helper;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Implement;
using InternalTrainingSystem.Core.Services.Interface;
using InternalTrainingSystem.Core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuthService authService,
            IUserService userService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
            _userService = userService;
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
                        Message = "Dữ liệu đầu vào không hợp lệ"
                    });
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || !user.IsActive)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Sai tài khoản hoặc mật khẩu"
                    });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    try
                    {
                        // Update last login date (Vietnam local time)
                        user.LastLoginDate = DateTimeUtils.Now();
                        await _userManager.UpdateAsync(user);

                        // Get user roles
                        var roles = await _userManager.GetRolesAsync(user);

                        // Generate JWT tokens
                        var accessToken = await _authService.GenerateAccessTokenAsync(user);
                        var refreshToken = _authService.GenerateRefreshToken(user.Id); // Pass userId

                        // Calculate expiry time
                        var expireMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRE_MINUTES") ?? "60");
                        var expiresAt = DateTimeUtils.Now().AddMinutes(expireMinutes);

                        var response = new LoginResponseDto
                        {
                            Success = true,
                            Message = "Đăng nhập thành công",
                            User = new UserProfileDto
                            {
                                Id = user.Id,
                                FullName = user.FullName,
                                Email = user.Email!,
                                EmployeeId = user.EmployeeId,
                                Department = user.Department?.Name,
                                Position = user.Position,
                                CurrentRole = roles.FirstOrDefault(),
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
                            Message = $"Đăng nhập thành công nhưng tạo token thất bại: {tokenEx.Message}"
                        });
                    }
                }

                if (result.IsLockedOut)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Tài khoản đã bị khóa do nhiều lần đăng nhập thất bại"
                    });
                }

                if (result.IsNotAllowed)
                {
                    return BadRequest(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Tài khoản không được phép đăng nhập"
                    });
                }

                return BadRequest(new LoginResponseDto
                {
                    Success = false,
                    Message = "Sai tài khoản hoặc mật khẩu"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new LoginResponseDto
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra khi đăng nhập: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Change password for current user
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                // Lấy user hiện tại từ token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Không thể xác định người dùng.");
                }

                // Xác nhận mật khẩu mới và xác nhận phải khớp
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest("Xác nhận mật khẩu không khớp.");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                // Kiểm tra mật khẩu hiện tại
                var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!isCurrentPasswordValid)
                {
                    return BadRequest("Mật khẩu hiện tại không chính xác.");
                }

                // Kiểm tra mật khẩu mới khác mật khẩu cũ
                if (request.CurrentPassword == request.NewPassword)
                {
                    return BadRequest("Mật khẩu mới phải khác mật khẩu hiện tại.");
                }

                // Tiến hành đổi mật khẩu
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (result.Succeeded)
                {
                    return Ok("Đổi mật khẩu thành công.");
                }

                // Gộp các lỗi từ Identity nếu có
                var errorMessages = result.Errors.Select(e => e.Description);
                return BadRequest(string.Join(", ", errorMessages));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi đổi mật khẩu: {ex.Message}");
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
                if (!string.IsNullOrEmpty(jwtId) && await _authService.IsTokenBlacklistedAsync(jwtId))
                {
                    return BadRequest(ApiResponseDto.ErrorResult("Người dùng đã đăng xuất"));
                }


                // Blacklist the current token
                if (!string.IsNullOrEmpty(jwtId))
                {
                    var expiry = DateTimeUtils.Now().AddDays(7); // Token expiry time (Vietnam local time)
                    await _authService.BlacklistTokenAsync(jwtId, expiry);
                }

                await _signInManager.SignOutAsync();

                return Ok(ApiResponseDto.SuccessResult(null, "Đăng xuất thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDto.ErrorResult($"Có lỗi xảy ra khi đăng xuất: {ex.Message}"));
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
                        Message = "Refresh token không hợp lệ"
                    });
                }

                // Validate refresh token using JWT service
                var tokenResponse = await _authService.RefreshTokenAsync(request.RefreshToken);

                if (tokenResponse == null || !tokenResponse.Success)
                {
                    return Unauthorized(new LoginResponseDto
                    {
                        Success = false,
                        Message = tokenResponse?.Message ?? "Refresh token không hợp lệ hoặc đã hết hạn"
                    });
                }

                // Get user information for response
                var user = await _userManager.FindByIdAsync(tokenResponse.UserId!);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new LoginResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy người dùng hoặc tài khoản không hoạt động"
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);

                var response = new LoginResponseDto
                {
                    Success = true,
                    Message = "Làm mới token thành công",
                    User = new UserProfileDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email!,
                        EmployeeId = user.EmployeeId,
                        Department = user.Department?.Name,
                        Position = user.Position,
                        CurrentRole = roles.FirstOrDefault(),
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
                    Message = $"Có lỗi xảy ra khi làm mới token: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Forgot password - Send OTP to email
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ForgotPasswordResponseDto>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ForgotPasswordResponseDto
                    {
                        Success = false,
                        Message = "Email không hợp lệ"
                    });
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null || !user.IsActive)
                {
                    return BadRequest(new ForgotPasswordResponseDto
                    {
                        Success = false,
                        Message = "Email không tồn tại trong hệ thống"
                    });
                }

                // Generate OTP
                var otpCode = OtpUtils.GenerateOtp();
                var otpExpiry = DateTimeUtils.Now().AddMinutes(5); // OTP expires in 5 minutes (Vietnam local time)

                // Update user with OTP
                user.OtpCode = otpCode;
                user.OtpExpiry = otpExpiry;
                await _userManager.UpdateAsync(user);

                // Send OTP email
                var subject = "Mã OTP Đặt lại Mật khẩu - Hệ thống Đào tạo Nội bộ";
                var htmlMessage = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background-color: #007bff; color: white; padding: 20px; text-align: center;'>
                            <h1>Đặt lại Mật khẩu</h1>
                        </div>
                        <div style='padding: 20px; background-color: #f8f9fa;'>
                            <p>Xin chào <strong>{user.FullName}</strong>,</p>
                            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản <strong>{user.Email}</strong></p>
                            <div style='background-color: white; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                                <h2 style='color: #007bff; margin: 0;'>Mã OTP của bạn:</h2>
                                <h1 style='color: #dc3545; letter-spacing: 3px; margin: 10px 0;'>{otpCode}</h1>
                                <p style='color: #dc3545; margin: 0;'><strong>Mã này sẽ hết hạn sau 5 phút</strong></p>
                            </div>
                            <p><strong>Lưu ý:</strong></p>
                            <ul>
                                <li>Không chia sẻ mã OTP này với bất kỳ ai</li>
                                <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                                <li>Mã OTP chỉ có hiệu lực trong 5 phút</li>
                            </ul>
                        </div>
                        <div style='background-color: #6c757d; color: white; padding: 15px; text-align: center;'>
                            <p style='margin: 0;'>© {DateTime.Now.Year} - Hệ thống Đào tạo Nội bộ</p>
                            <p style='margin: 0;'>Email hỗ trợ: support@company.com</p>
                        </div>
                    </div>";

                Hangfire.BackgroundJob.Enqueue(() => EmailHelper.SendEmailAsync(
                    user.Email!,
                    subject,
                    htmlMessage
                ));

                return Ok(new ForgotPasswordResponseDto
                {
                    Success = true,
                    Message = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư và nhập mã OTP để tiếp tục."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ForgotPasswordResponseDto
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra khi gửi OTP: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Verify OTP for password reset
        /// </summary>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<ActionResult<VerifyOtpResponseDto>> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new VerifyOtpResponseDto
                    {
                        Success = false,
                        Message = "Dữ liệu không hợp lệ"
                    });
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new VerifyOtpResponseDto
                    {
                        Success = false,
                        Message = "Email không tồn tại trong hệ thống"
                    });
                }

                // Check if OTP exists and not expired
                if (string.IsNullOrEmpty(user.OtpCode) || user.OtpExpiry == null)
                {
                    return BadRequest(new VerifyOtpResponseDto
                    {
                        Success = false,
                        Message = "Không tìm thấy mã OTP. Vui lòng yêu cầu gửi lại mã."
                    });
                }

                if (DateTimeUtils.Now() > user.OtpExpiry)
                {
                    return BadRequest(new VerifyOtpResponseDto
                    {
                        Success = false,
                        Message = "Mã OTP đã hết hạn. Vui lòng yêu cầu gửi lại mã."
                    });
                }

                if (user.OtpCode != request.Otp)
                {
                    return BadRequest(new VerifyOtpResponseDto
                    {
                        Success = false,
                        Message = "Mã OTP không chính xác"
                    });
                }

                // Generate password reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                return Ok(new VerifyOtpResponseDto
                {
                    Success = true,
                    Message = "Xác minh OTP thành công",
                    ResetToken = resetToken
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new VerifyOtpResponseDto
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra khi xác minh OTP: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Reset password after OTP verification
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseDto>> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponseDto.ErrorResult("Dữ liệu không hợp lệ"));
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(ApiResponseDto.ErrorResult("Email không tồn tại trong hệ thống"));
                }

                // Verify OTP again for security
                if (string.IsNullOrEmpty(user.OtpCode) || user.OtpExpiry == null)
                {
                    return BadRequest(ApiResponseDto.ErrorResult("Phiên đặt lại mật khẩu đã hết hạn"));
                }

                if (DateTimeUtils.Now() > user.OtpExpiry)
                {
                    return BadRequest(ApiResponseDto.ErrorResult("Mã OTP đã hết hạn"));
                }

                if (user.OtpCode != request.Otp)
                {
                    return BadRequest(ApiResponseDto.ErrorResult("Mã OTP không chính xác"));
                }

                // Generate new password
                var newPassword = PasswordUtils.Generate(_userManager.Options.Password);

                // Reset password using token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(ApiResponseDto.ErrorResult("Không thể đặt lại mật khẩu", errors));
                }

                // Clear OTP after successful reset
                user.OtpCode = null;
                user.OtpExpiry = null;
                await _userManager.UpdateAsync(user);

                // Send new password via email
                var subject = "Mật khẩu Mới - Hệ thống Đào tạo Nội bộ";
                var htmlMessage = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background-color: #28a745; color: white; padding: 20px; text-align: center;'>
                            <h1>Đặt lại Mật khẩu Thành công</h1>
                        </div>
                        <div style='padding: 20px; background-color: #f8f9fa;'>
                            <p>Xin chào <strong>{user.FullName}</strong>,</p>
                            <p>Mật khẩu của bạn đã được đặt lại thành công.</p>
                            <div style='background-color: white; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                                <h3 style='color: #28a745; margin: 0;'>Mật khẩu mới của bạn:</h3>
                                <h2 style='color: #dc3545; letter-spacing: 2px; margin: 10px 0; font-family: monospace;'>{newPassword}</h2>
                                <p style='color: #dc3545; margin: 0;'><strong>Vui lòng đổi mật khẩu sau khi đăng nhập</strong></p>
                            </div>
                            <p><strong>Thông tin đăng nhập:</strong></p>
                            <ul>
                                <li><strong>Email:</strong> {user.Email}</li>
                                <li><strong>Mật khẩu:</strong> {newPassword}</li>
                            </ul>
                            <p><strong>Lưu ý bảo mật:</strong></p>
                            <ul>
                                <li>Vui lòng đổi mật khẩu ngay sau khi đăng nhập</li>
                                <li>Không chia sẻ thông tin đăng nhập với bất kỳ ai</li>
                                <li>Sử dụng mật khẩu mạnh để bảo vệ tài khoản</li>
                            </ul>
                            <div style='text-align: center; margin: 20px 0;'>
                                <a href='#' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Đăng nhập ngay</a>
                            </div>
                        </div>
                        <div style='background-color: #6c757d; color: white; padding: 15px; text-align: center;'>
                            <p style='margin: 0;'>© {DateTime.Now.Year} - Hệ thống Đào tạo Nội bộ</p>
                            <p style='margin: 0;'>Email hỗ trợ: support@company.com</p>
                        </div>
                    </div>";

                Hangfire.BackgroundJob.Enqueue(() => EmailHelper.SendEmailAsync(
                    user.Email!,
                    subject,
                    htmlMessage
                ));

                return Ok(ApiResponseDto.SuccessResult(null, "Đặt lại mật khẩu thành công. Mật khẩu mới đã được gửi đến email của bạn."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseDto.ErrorResult($"Có lỗi xảy ra khi đặt lại mật khẩu: {ex.Message}"));
            }
        }

    }
}