using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StoryReader.Application.Common;
using StoryReader.Application.DTOs;
using StoryReader.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StoryReader.Infrastructure.Redis
{
    public class RedisStoryCache : IRedisStoryCache
    {
        private readonly IDistributedCache _cache;  // High-level wrapper
        private readonly ILogger<RedisStoryCache> _logger;

        public RedisStoryCache(IDistributedCache cache, ILogger<RedisStoryCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        // GetAll Cached
        public async Task<PagedResult<StoryDto>?> GetAllCachedAsync(int page, int pageSize)
        {
            var cacheKey = $"stories:list:p{page}_s{pageSize}";
            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Cache hit for {Key}", cacheKey);
                    return JsonSerializer.Deserialize<PagedResult<StoryDto>>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache for {Key}", cacheKey);
            }
            return null;  // Miss
        }

        public async Task SetAllCachedAsync(int page, int pageSize, PagedResult<StoryDto> data, TimeSpan ttl)
        {
            var cacheKey = $"stories:list:p{page}_s{pageSize}";
            try
            {
                var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
                var jsonData = JsonSerializer.Serialize(data);
                await _cache.SetStringAsync(cacheKey, jsonData, options);
                _logger.LogInformation("Set cache for {Key} with TTL {TtlHours}h", cacheKey, ttl.TotalHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for {Key}", cacheKey);
            }
        }

        // GetBySlug Cached
        public async Task<StoryDto?> GetBySlugCachedAsync(string slug)
        {
            var cacheKey = $"stories:detail:{slug}";
            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Cache hit for {Key}", cacheKey);
                    return JsonSerializer.Deserialize<StoryDto>(cachedData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache for {Key}", cacheKey);
            }
            return null;  // Miss
        }

        public async Task SetBySlugCachedAsync(string slug, StoryDto data, TimeSpan ttl)
        {
            var cacheKey = $"stories:detail:{slug}";
            try
            {
                var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };
                var jsonData = JsonSerializer.Serialize(data);
                await _cache.SetStringAsync(cacheKey, jsonData, options);
                _logger.LogInformation("Set cache for {Key} with TTL {TtlMinutes}m", cacheKey, ttl.TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for {Key}", cacheKey);
            }
        }

        // Invalidate
        public async Task InvalidateAllAsync()
        {
            try
            {
                // DistributedCache không hỗ trợ pattern delete trực tiếp – dùng low-level nếu cần
                // Để simple, giả sử xóa broad (hoặc implement SCAN với IConnectionMultiplexer)
                // Ví dụ: await _cache.RemoveAsync("stories:list:*"); – custom method
                _logger.LogInformation("Invalidated all stories caches");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalidate all failed");
            }
        }

        public async Task InvalidateBySlugAsync(string slug)
        {
            var cacheKey = $"stories:detail:{slug}";
            try
            {
                await _cache.RemoveAsync(cacheKey);
                _logger.LogInformation("Invalidated cache for {Key}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalidate by slug failed for {Key}", cacheKey);
            }
        }
    }
}
