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

        public StoryService(IStoryRepository repo)
        {
            _repo = repo;
        }

        // ---------- CREATE ----------
        public async Task<StoryDto> CreateAsync(Guid authorId, CreateStoryRequest request)
        {
            var slug = GenerateSlug(request.Title);

            if (await _repo.ExistsBySlugAsync(slug))
                throw AppException.Conflict(
                    "STORY_EXISTS",
                    "Story with same title already exists");

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

            return ToDto(story);
        }

        // ---------- READ ----------
        public async Task<PagedResult<StoryDto>> GetAllAsync(int page, int pageSize)
        {
            if (page <= 0 || pageSize <= 0)
                throw AppException.BadRequest(
                    "INVALID_PAGING",
                    "Page and pageSize must be greater than zero");

            var stories = await _repo.GetAllAsync(page, pageSize);
            var total = await _repo.CountAsync();

            return new PagedResult<StoryDto>
            {
                Items = stories.Select(ToDto).ToList(),
                Page = page,
                PageSize = pageSize,
                Total = total
            };
        }

        public async Task<StoryDto> GetBySlugAsync(string slug)
        {
            var story = await _repo.GetBySlugAsync(slug);

            if (story == null)
                throw AppException.NotFound(
                    "STORY_NOT_FOUND",
                    "Story not found");

            await _repo.IncrementViewsAsync(story.Id);
            story.Views += 1;

            return ToDto(story);
        }

        // ---------- HELPERS ----------
        private static StoryDto ToDto(Story story) => new()
        {
            Id = story.Id,
            Title = story.Title,
            Slug = story.Slug,
            Description = story.Description,
            Views = story.Views
        };

        private static string GenerateSlug(string title)
            => title.Trim().ToLower().Replace(" ", "-");
    }
}
