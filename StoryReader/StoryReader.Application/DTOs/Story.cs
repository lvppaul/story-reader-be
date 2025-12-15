using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Application.DTOs
{
    public class CreateStoryRequest
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CoverUrl { get; set; }
    }

    public class StoryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? Description { get; set; }
        public long Views { get; set; }
    }
}
