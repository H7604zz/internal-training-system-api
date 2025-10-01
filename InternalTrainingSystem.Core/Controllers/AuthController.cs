using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.DTOs;
using System.Security.Claims;
using InternalTrainingSystem.Core.Services.Implement;

namespace InternalTrainingSystem.Core.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;

        public AuthController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
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
                        var refreshToken = _jwtService.GenerateRefreshToken();
                        
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
                                Department = user.Department,
                                Position = user.Position,
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

        
    }
}