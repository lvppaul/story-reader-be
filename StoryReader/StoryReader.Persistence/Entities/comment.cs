using System;
using System.Collections.Generic;

namespace StoryReader.Persistence.Entities;

public partial class comment
{
    public Guid id { get; set; }

    public Guid? story_id { get; set; }

    public Guid? chapter_id { get; set; }

    public Guid? user_id { get; set; }

    public Guid? parent_id { get; set; }

    public string content { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public virtual ICollection<comment> Inverseparent { get; set; } = new List<comment>();

    public virtual chapter? chapter { get; set; }

    public virtual comment? parent { get; set; }

    public virtual story? story { get; set; }

    public virtual user? user { get; set; }
}
