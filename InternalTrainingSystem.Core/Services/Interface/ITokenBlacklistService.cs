namespace InternalTrainingSystem.Core.Services.Interface
{
    public interface ITokenBlacklistService
    {
        Task BlacklistTokenAsync(string tokenId, DateTime expiry);
        Task<bool> IsTokenBlacklistedAsync(string tokenId);
        Task CleanupExpiredTokensAsync();
    }
}