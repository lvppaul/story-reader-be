using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StoryReader.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Infrastructure.Redis
{
    public class RedisAuthTokenStore : IAuthTokenStore
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisAuthTokenStore> _logger;

        public RedisAuthTokenStore(IConnectionMultiplexer redis, ILogger<RedisAuthTokenStore> logger)  // Thêm param logger vào ctor
        {
            _db = redis.GetDatabase();
            _logger = logger;  // Assign logger
        }

        private static string RefreshKey(string token)
            => $"refresh:{token}";

        private static string UserKey(Guid userId)
            => $"user_refresh:{userId}";

        private static string BlacklistKey(string token)
            => $"blacklist_refresh:{token}";

        public async Task SaveAsync(string token, Guid userId, TimeSpan ttl)
        {
            await _db.StringSetAsync(RefreshKey(token), userId.ToString(), ttl);
            var added = await _db.SetAddAsync(UserKey(userId), token);  // Trả về 1 nếu added mới, 0 nếu duplicate
            _logger.LogInformation("Saved token {Token} for user {UserId}: Added to set? {Added}, TTL {TtlDays} days",
                                   token.Substring(0, 8) + "...", userId, added, ttl.TotalDays);  // Log partial token cho safe
        }

        public async Task<Guid?> GetUserIdAsync(string token)
        {
            var value = await _db.StringGetAsync(RefreshKey(token));
            if (value.HasValue && Guid.TryParse(value.ToString(), out var guid))  
                return guid;
            return null;
        }

        public async Task RevokeAsync(string token)
        {
            await _db.KeyDeleteAsync(RefreshKey(token));
        }

        public async Task RevokeAllAsync(Guid userId)
        {
            var key = UserKey(userId);
            var members = await _db.SetMembersAsync(key);
            var validTokens = members.Where(m => m.HasValue).Select(m => m!.ToString());  // Filter nulls
            await Task.WhenAll(validTokens.Select(token => _db.KeyDeleteAsync(RefreshKey(token))));  // Parallel delete
            await _db.KeyDeleteAsync(key);
            _logger.LogInformation("Revoked all tokens for user {UserId}: Deleted {Count} tokens", userId, validTokens.Count());  // Thêm log cho debug
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            return await _db.KeyExistsAsync(BlacklistKey(token));
        }

        public async Task BlacklistAsync(string token, TimeSpan ttl)
        {
            await _db.StringSetAsync(
                BlacklistKey(token),
                "1",
                ttl);
        }
    }
}
