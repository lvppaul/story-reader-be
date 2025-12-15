using Microsoft.EntityFrameworkCore;
using StoryReader.Application.Interfaces;
using StoryReader.Domain.Entities;
using StoryReader.Persistence.Context;
using StoryReader.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Persistence.Repositories
{
    public class StoryRepository : IStoryRepository
    {
        private readonly AppDbContext _db;

        public StoryRepository(AppDbContext db)
        {
            _db = db;
        }

        // ---------- CREATE ----------
        public async Task AddAsync(Story story)
        {
            var entity = ToEntity(story);
            _db.stories.Add(entity);
            await _db.SaveChangesAsync();
        }

        // ---------- READ ----------
        public async Task<Story?> GetBySlugAsync(string slug)
        {
            var entity = await _db.stories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.slug == slug);

            return entity == null ? null : ToDomain(entity);
        }

        public async Task<List<Story>> GetAllAsync(int page, int pageSize)
        {
            var entities = await _db.stories
                .Where(x => x.is_published == true)
                .OrderByDescending(x => x.created_at)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return entities.Select(ToDomain).ToList();
        }

        public async Task<long> CountAsync()
        {
            return await _db.stories
                .Where(x => x.is_published == true)
                .LongCountAsync();
        }

        public async Task<bool> ExistsBySlugAsync(string slug)
        {
            return await _db.stories.AnyAsync(x => x.slug == slug);
        }

        public async Task IncrementViewsAsync(Guid storyId)
        {
            await _db.stories
                .Where(x => x.id == storyId)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.views, x => x.views + 1));
        }

        // ---------- MAPPING ----------
        private static Story ToDomain(story e) => new()
        {
            Id = e.id,
            AuthorId = e.author_id,
            CategoryId = e.category_id,
            Title = e.title,
            Slug = e.slug,
            Description = e.description,
            CoverUrl = e.cover_url,
            IsPublished = e.is_published ?? true,
            Views = e.views ?? 0,
            CreatedAt = e.created_at ?? DateTime.UtcNow,
            UpdatedAt = e.updated_at ?? DateTime.UtcNow
        };

        private static story ToEntity(Story s) => new()
        {
            id = s.Id,
            author_id = s.AuthorId,
            category_id = s.CategoryId,
            title = s.Title,
            slug = s.Slug,
            description = s.Description,
            cover_url = s.CoverUrl,
            is_published = s.IsPublished,
            views = s.Views,
            created_at = s.CreatedAt,
            updated_at = s.UpdatedAt
        };
    }
}
