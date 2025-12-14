using System;
using System.Collections.Generic;

namespace StoryReader.Persistence.Entities;

public partial class reading_progress
{
    public Guid id { get; set; }

    public Guid? user_id { get; set; }

    public Guid? story_id { get; set; }

    public Guid? chapter_id { get; set; }

    public int? position { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual chapter? chapter { get; set; }

    public virtual story? story { get; set; }

    public virtual user? user { get; set; }
}
