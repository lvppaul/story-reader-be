using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryReader.Domain.Entities
{
    public class Story
    {
        public Guid Id { get; set; }

        public Guid? AuthorId { get; set; }
        public Guid? CategoryId { get; set; }

        public string Title { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public string? Description { get; set; }
        public string? CoverUrl { get; set; }

        public string Status { get; set; } = "ongoing";
        public bool IsPublished { get; set; } = true;

        public long Views { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
