using Microsoft.EntityFrameworkCore;
using StoryReader.Application.Interfaces;
using StoryReader.Domain.Entities;
using StoryReader.Persistence.Context;
using StoryReader.Persistence.Entities;
using StoryReader.Persistence.Mappers;

namespace StoryReader.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByEmailAsync(string normalizedEmail)
        {
            var entity = await _db.users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.normalized_email == normalizedEmail);

            return entity?.ToDomain();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            var entity = await _db.users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.id == id);

            return entity?.ToDomain();
        }

        public async Task<bool> ExistsByEmailAsync(string normalizedEmail)
        {
            return await _db.users.AnyAsync(x => x.normalized_email == normalizedEmail);
        }

        public async Task AddAsync(User user)
        {
            var entity = new user
            {
                id = user.Id,
                email = user.Email,
                normalized_email = user.NormalizedEmail,
                password_hash = user.PasswordHash,
                display_name = user.DisplayName,
                role = user.Role,
                is_active = true,
                is_email_confirmed = false,
                created_at = user.CreatedAt,
                updated_at = user.UpdatedAt
            };

            _db.users.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            var entity = await _db.users.FirstAsync(x => x.id == user.Id);

            entity.display_name = user.DisplayName;
            entity.role = user.Role;
            entity.is_active = user.IsActive;
            entity.is_email_confirmed = user.IsEmailConfirmed;
            entity.last_login_at = user.LastLoginAt;
            entity.updated_at = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
