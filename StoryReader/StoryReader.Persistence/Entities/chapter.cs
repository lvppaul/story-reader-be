using System;
using System.Collections.Generic;

namespace StoryReader.Persistence.Entities;

public partial class chapter
{
    public Guid id { get; set; }

    public Guid story_id { get; set; }

    public string? title { get; set; }

    public decimal chapter_number { get; set; }

    public string content { get; set; } = null!;

    public int? word_count { get; set; }

    public bool? is_premium { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual ICollection<comment> comments { get; set; } = new List<comment>();

    public virtual ICollection<reading_progress> reading_progresses { get; set; } = new List<reading_progress>();

    public virtual story story { get; set; } = null!;
}
