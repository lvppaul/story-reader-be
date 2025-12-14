using System;
using System.Collections.Generic;

namespace StoryReader.Persistence.Entities;

public partial class favorite
{
    public Guid user_id { get; set; }

    public Guid story_id { get; set; }

    public DateTime? created_at { get; set; }

    public virtual story story { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
