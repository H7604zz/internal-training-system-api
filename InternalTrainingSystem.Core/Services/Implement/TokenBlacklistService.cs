using InternalTrainingSystem.Core.Services.Interface;
using System.Collections.Concurrent;

namespace InternalTrainingSystem.Core.Services.Implement
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        // In-memory cache for blacklisted tokens
        // In production, you should use Redis or database
        private static readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

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
            var now = DateTime.Now;
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