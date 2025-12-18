using Microsoft.Extensions.Logging;
using StoryReader.Application.Common;
using StoryReader.Application.DTOs;
using StoryReader.Application.Interfaces;
using StoryReader.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Services
{
    public class StoryService : IStoryService
    {
        private readonly IStoryRepository _repo;
        private readonly IRedisStoryCache _cache;  // Depend vào interface (Application)
        private readonly ILogger<StoryService> _logger;

        public StoryService(IStoryRepository repo, IRedisStoryCache cache, ILogger<StoryService> logger)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
        }

        public async Task<StoryDto> CreateAsync(Guid authorId, CreateStoryRequest request)
        {
            var slug = GenerateSlug(request.Title);
            if (await _repo.ExistsBySlugAsync(slug))
                throw AppException.Conflict("STORY_EXISTS", "Story with same title already exists");

            var story = new Story
            {
                Id = Guid.NewGuid(),
                AuthorId = authorId,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Slug = slug,
                Description = request.Description,
                CoverUrl = request.CoverUrl,
                IsPublished = true,
                Views = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(story);

            // Invalidate qua interface (không biết Redis)
            await _cache.InvalidateAllAsync();

            return ToDto(story);
        }

        public async Task<PagedResult<StoryDto>> GetAllAsync(int page, int pageSize)
        {
            if (page <= 0 || pageSize <= 0)
                throw AppException.BadRequest("INVALID_PAGING", "Page and pageSize must be greater than zero");

            // Check cache qua interface
            var cachedResult = await _cache.GetAllCachedAsync(page, pageSize);
            if (cachedResult != null)
                return cachedResult;

            // Miss: Load DB
            _logger.LogInformation("Loading stories from DB (cache miss)");
            var stories = await _repo.GetAllAsync(page, pageSize);
            var total = await _repo.CountAsync();
            var result = new PagedResult<StoryDto>
            {
                Items = stories.Select(ToDto).ToList(),
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            // Set cache qua interface
            await _cache.SetAllCachedAsync(page, pageSize, result, TimeSpan.FromHours(1));

            return result;
        }

        public async Task<StoryDto> GetBySlugAsync(string slug)
        {
            // Check cache qua interface
            var cachedDto = await _cache.GetBySlugCachedAsync(slug);
            if (cachedDto != null)
            {
                // Increment views từ DB (dynamic)
                await _repo.IncrementViewsAsync(cachedDto.Id);  // Cần map Id từ DTO
                cachedDto.Views += 1;  // Update local
                return cachedDto;
            }

            // Miss: Load DB
            _logger.LogInformation("Loading story from DB (cache miss)");
            var story = await _repo.GetBySlugAsync(slug);
            if (story == null)
                throw AppException.NotFound("STORY_NOT_FOUND", "Story not found");

            // Increment views
            await _repo.IncrementViewsAsync(story.Id);
            story.Views += 1;

            var dto = ToDto(story);

            // Set cache qua interface
            await _cache.SetBySlugCachedAsync(slug, dto, TimeSpan.FromMinutes(30));

            return dto;
        }

        // Helpers (giữ nguyên)
        private static StoryDto ToDto(Story story) => new()
        {
            Id = story.Id,
            Title = story.Title,
            Slug = story.Slug,
            Description = story.Description,
            Views = story.Views
        };

        private static string GenerateSlug(string title) => title.Trim().ToLower().Replace(" ", "-");
    }
}
