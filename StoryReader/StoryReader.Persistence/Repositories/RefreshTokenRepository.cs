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
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _db;

        public RefreshTokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(RefreshToken token)
        {
            var entity = new refresh_token
            {
                id = token.Id,
                user_id = token.UserId,
                token = token.Token,
                created_at = token.CreatedAt,
                expires_at = token.ExpiresAt
            };

            _db.refresh_tokens.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            var entity = await _db.refresh_tokens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.token == token);

            if (entity == null) return null;

            return new RefreshToken
            {
                Id = entity.id,
                UserId = entity.user_id,
                Token = entity.token,
                CreatedAt = entity.created_at ?? DateTime.UtcNow,
                ExpiresAt = entity.expires_at,
                RevokedAt = entity.revoked_at
            };
        }

        public async Task RevokeAsync(Guid tokenId)
        {
            var entity = await _db.refresh_tokens.FirstAsync(x => x.id == tokenId);
            entity.revoked_at = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task RevokeAllByUserAsync(Guid userId)
        {
            var tokens = await _db.refresh_tokens
                .Where(x => x.user_id == userId && x.revoked_at == null)
                .ToListAsync();

            foreach (var t in tokens)
            {
                t.revoked_at = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }
    }

}
