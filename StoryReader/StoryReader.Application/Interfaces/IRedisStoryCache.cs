using StoryReader.Application.Common;
using StoryReader.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Interfaces
{
    public interface IRedisStoryCache
    {
        // Get/Set cho GetAll (paged)
        Task<PagedResult<StoryDto>?> GetAllCachedAsync(int page, int pageSize);
        Task SetAllCachedAsync(int page, int pageSize, PagedResult<StoryDto> data, TimeSpan ttl);

        // Get/Set cho GetBySlug
        Task<StoryDto?> GetBySlugCachedAsync(string slug);
        Task SetBySlugCachedAsync(string slug, StoryDto data, TimeSpan ttl);

        // Invalidate
        Task InvalidateAllAsync();  // Xóa all list caches
        Task InvalidateBySlugAsync(string slug);  // Xóa 1 detail
    }
}
