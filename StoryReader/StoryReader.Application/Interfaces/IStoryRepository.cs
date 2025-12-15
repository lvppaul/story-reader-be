using StoryReader.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Interfaces
{
    public interface IStoryRepository
    {
        Task AddAsync(Story story);
        Task<Story?> GetBySlugAsync(string slug);
        Task<List<Story>> GetAllAsync(int page, int pageSize);

        Task<long> CountAsync();

        Task<bool> ExistsBySlugAsync(string slug);
        Task IncrementViewsAsync(Guid storyId);
    }
}
