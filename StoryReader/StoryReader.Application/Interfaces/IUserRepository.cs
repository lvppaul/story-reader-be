using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoryReader.Domain.Entities;

namespace StoryReader.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string normalizedEmail);
        Task<User?> GetByIdAsync(Guid id);
        Task<bool> ExistsByEmailAsync(string normalizedEmail);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }


}
