using StoryReader.Domain.Entities;
using StoryReader.Persistence.Entities;

namespace StoryReader.Persistence.Mappers
{
    public static class UserMapper
    {
        public static User ToDomain(this user entity)
        {
            return new User
            {
                Id = entity.id,
                Email = entity.email,
                NormalizedEmail = entity.normalized_email,
                PasswordHash = entity.password_hash,
                DisplayName = entity.display_name,
                Role = entity.role ?? "user",
                IsEmailConfirmed = entity.is_email_confirmed,
                IsActive = entity.is_active,
                CreatedAt = entity.created_at,
                UpdatedAt = entity.updated_at,
                LastLoginAt = entity.last_login_at
            };
        }

        public static user ToEntity(this User domain)
        {
            return new user
            {
                id = domain.Id,
                email = domain.Email,
                normalized_email = domain.NormalizedEmail,
                password_hash = domain.PasswordHash,
                display_name = domain.DisplayName,
                role = domain.Role,
                is_email_confirmed = domain.IsEmailConfirmed,
                is_active = domain.IsActive,
                created_at = domain.CreatedAt,
                updated_at = domain.UpdatedAt,
                last_login_at = domain.LastLoginAt
            };
        }
    }
}