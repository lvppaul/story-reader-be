using System;
using System.Collections.Generic;

namespace StoryReader.Persistence.Entities;

public partial class refresh_token
{
    public Guid id { get; set; }

    public Guid user_id { get; set; }

    public string token { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime expires_at { get; set; }

    public DateTime? revoked_at { get; set; }

    public virtual user user { get; set; } = null!;
}
