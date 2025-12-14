using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace StoryReader.Persistence.Entities;

public partial class story
{
    public Guid id { get; set; }

    public Guid? author_id { get; set; }

    public string title { get; set; } = null!;

    public string slug { get; set; } = null!;

    public string? description { get; set; }

    public Guid? category_id { get; set; }

    public string? cover_url { get; set; }

    public string? status { get; set; }

    public bool? is_published { get; set; }

    public long? views { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public NpgsqlTsVector? search_vector { get; set; }

    public virtual user? author { get; set; }

    public virtual category? category { get; set; }

    public virtual ICollection<chapter> chapters { get; set; } = new List<chapter>();

    public virtual ICollection<comment> comments { get; set; } = new List<comment>();

    public virtual ICollection<favorite> favorites { get; set; } = new List<favorite>();

    public virtual ICollection<rating> ratings { get; set; } = new List<rating>();

    public virtual ICollection<reading_progress> reading_progresses { get; set; } = new List<reading_progress>();

    public virtual ICollection<tag> tags { get; set; } = new List<tag>();
}
