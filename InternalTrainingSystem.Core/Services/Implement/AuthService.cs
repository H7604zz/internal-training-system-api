using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using InternalTrainingSystem.Core.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InternalTrainingSystem.Core.Services.Implement
{

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private static readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

        public AuthService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            try
            {
                var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "InternalTrainingSystem2024@SecretKey!MinimumLength32Characters";
                var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "InternalTrainingSystem.API";
                var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "InternalTrainingSystem.Client";
                var expireMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRE_MINUTES") ?? "60");

                Console.WriteLine($"Generating token for user: {user.Email}");
                Console.WriteLine($"Secret key length: {secretKey.Length}");

                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("JWT Secret Key is not configured");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var roles = await _userManager.GetRolesAsync(user);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Name, user.FullName ?? ""),
                    new Claim("EmployeeId", user.EmployeeId ?? ""),
                    new Claim("Department", user.Department ?.Name ?? ""),
                    new Claim("Position", user.Position ?? ""),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Add roles as claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(expireMinutes),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                Console.WriteLine($"Token generated successfully, length: {tokenString.Length}");
                
                return tokenString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating JWT token: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // Temporary in-memory storage for refresh tokens
        // In production, use database or Redis
        private static readonly Dictionary<string, string> _refreshTokenStore = new();

        public string GenerateRefreshToken(string userId)
        {
            var randomBytes = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = Convert.ToBase64String(randomBytes);
            
            // Store the mapping between refresh token and user ID
            _refreshTokenStore[refreshToken] = userId;
            
            return refreshToken;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSettings["SecretKey"];
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtSettings["Issuer"];
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtSettings["Audience"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey!);

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // In a simplified implementation, we'll validate the refresh token format
                // In production, you should store refresh tokens in database with expiry dates
                
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return new TokenResponseDto
                    {
                        Success = false,
                        Message = "Refresh token is required"
                    };
                }

                // For this example, we'll decode the refresh token to extract user info
                // In production, you should look up the refresh token in your database
                try
                {
                    var refreshTokenBytes = Convert.FromBase64String(refreshToken);
                    var refreshTokenString = System.Text.Encoding.UTF8.GetString(refreshTokenBytes);
                    
                    // Simple validation - in production, check against database
                    if (refreshTokenString.Length < 10)
                    {
                        return new TokenResponseDto
                        {
                            Success = false,
                            Message = "Invalid refresh token format"
                        };
                    }

                    // For demo purposes, we'll extract userId from token claims
                    // In production, get userId from database based on refresh token
                    var userId = ExtractUserIdFromRefreshToken(refreshToken);
                    
                    if (string.IsNullOrEmpty(userId))
                    {
                        return new TokenResponseDto
                        {
                            Success = false,
                            Message = "Could not extract user from refresh token"
                        };
                    }

                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null || !user.IsActive)
                    {
                        return new TokenResponseDto
                        {
                            Success = false,
                            Message = "User not found or inactive"
                        };
                    }

                    // Generate new tokens
                    var newAccessToken = await GenerateAccessTokenAsync(user);
                    var newRefreshToken = GenerateRefreshToken(user.Id); // Pass userId for new refresh token
                    
                    // Invalidate old refresh token
                    _refreshTokenStore.Remove(refreshToken);
                    
                    var expireMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRE_MINUTES") ?? "60");
                    var expiresAt = DateTime.Now.AddMinutes(expireMinutes);

                    return new TokenResponseDto
                    {
                        Success = true,
                        Message = "Token refreshed successfully",
                        UserId = user.Id,
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        ExpiresAt = expiresAt
                    };
                }
                catch (FormatException)
                {
                    return new TokenResponseDto
                    {
                        Success = false,
                        Message = "Invalid refresh token format"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RefreshTokenAsync: {ex.Message}");
                return new TokenResponseDto
                {
                    Success = false,
                    Message = "Internal error occurred while refreshing token"
                };
            }
        }

        private string ExtractUserIdFromRefreshToken(string refreshToken)
        {
            try
            {
                // Look up user ID from our in-memory store
                // In production, query database for refresh token
                if (_refreshTokenStore.TryGetValue(refreshToken, out var userId))
                {
                    return userId;
                }
                
                Console.WriteLine($"Refresh token not found in store: {refreshToken.Substring(0, Math.Min(10, refreshToken.Length))}...");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting user ID from refresh token: {ex.Message}");
                return string.Empty;
            }
        }
        public Task BlacklistTokenAsync(string tokenId, DateTime expiry)
        {
            _blacklistedTokens.TryAdd(tokenId, expiry);
            return Task.CompletedTask;
        }

        public Task<bool> IsTokenBlacklistedAsync(string tokenId)
        {
            return Task.FromResult(_blacklistedTokens.ContainsKey(tokenId));
        }

        public Task CleanupExpiredTokensAsync()
        {
            var now = DateTime.UtcNow;
            var expiredTokens = _blacklistedTokens
                .Where(kvp => kvp.Value <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var tokenId in expiredTokens)
            {
                _blacklistedTokens.TryRemove(tokenId, out _);
            }

            return Task.CompletedTask;
        }
    }
}