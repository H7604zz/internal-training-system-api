using InternalTrainingSystem.Core.DTOs;
using InternalTrainingSystem.Core.Models;
using System.Security.Claims;

namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface IAuthService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);
        string GenerateRefreshToken();
        string GenerateRefreshToken(string userId);
        ClaimsPrincipal? ValidateToken(string token);
        Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);

        Task BlacklistTokenAsync(string tokenId, DateTime expiry);
        Task<bool> IsTokenBlacklistedAsync(string tokenId);
        Task CleanupExpiredTokensAsync();
    }
}
