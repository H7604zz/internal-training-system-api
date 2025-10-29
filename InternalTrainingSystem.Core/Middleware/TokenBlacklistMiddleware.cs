using InternalTrainingSystem.Core.Services.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public TokenBlacklistMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip blacklist check for non-authenticated endpoints
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                try
                {
                    var jwtHandler = new JwtSecurityTokenHandler();
                    var jsonToken = jwtHandler.ReadJwtToken(token);
                    var jwtId = jsonToken?.Claims?.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;

                    if (!string.IsNullOrEmpty(jwtId))
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var blacklistService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                        
                        if (await blacklistService.IsTokenBlacklistedAsync(jwtId))
                        {
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            
                            var errorResponse = new
                            {
                                success = false,
                                message = "Token has been invalidated. Please login again.",
                                data = (object?)null,
                                errors = new[] { "BLACKLISTED_TOKEN" }
                            };
                            
                            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
                            return;
                        }
                    }
                }
                catch
                {
                    // If token parsing fails, let it pass through to normal JWT validation
                }
            }

            await _next(context);
        }
    }
}