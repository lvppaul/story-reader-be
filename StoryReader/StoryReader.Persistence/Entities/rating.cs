using System;
using System.Collections.Generic;

namespace StoryReader.Persistence.Entities;

public partial class rating
{
    public Guid id { get; set; }

    public Guid? story_id { get; set; }

    public Guid? user_id { get; set; }

    public short score { get; set; }

    public DateTime? created_at { get; set; }

    public virtual story? story { get; set; }

    public virtual user? user { get; set; }
}
