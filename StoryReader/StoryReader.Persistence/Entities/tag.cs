using System;
using System.Collections.Generic;

namespace StoryReader.Persistence.Entities;

public partial class tag
{
    public Guid id { get; set; }

    public string name { get; set; } = null!;

    public string slug { get; set; } = null!;

    public virtual ICollection<story> stories { get; set; } = new List<story>();
}
