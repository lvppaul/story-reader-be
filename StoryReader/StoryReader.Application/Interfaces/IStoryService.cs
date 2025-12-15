using StoryReader.Application.Common;
using StoryReader.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.Interfaces
{
    public interface IStoryService
    {
        Task<StoryDto> CreateAsync(Guid authorId, CreateStoryRequest request);

        Task<PagedResult<StoryDto>> GetAllAsync(int page, int pageSize);


        Task<StoryDto> GetBySlugAsync(string slug);
    }
}
