using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Interfaces
{
    public interface IAuthTokenStore
    {
        Task SaveAsync(string token, Guid userId, TimeSpan ttl);
        Task<Guid?> GetUserIdAsync(string token);

        Task RevokeAsync(string token);
        Task RevokeAllAsync(Guid userId);

        Task<bool> IsBlacklistedAsync(string token);
        Task BlacklistAsync(string token, TimeSpan ttl);
    }
}
