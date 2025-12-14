using System;
using System.Collections.Generic;

namespace StoryReader.Persistence.Entities;

public partial class user
{
    public Guid id { get; set; }

    public string email { get; set; } = null!;

    public string normalized_email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public string? display_name { get; set; }

    public bool? is_email_confirmed { get; set; }

    public bool? is_active { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public DateTime? last_login_at { get; set; }

    public virtual ICollection<comment> comments { get; set; } = new List<comment>();

    public virtual ICollection<favorite> favorites { get; set; } = new List<favorite>();

    public virtual ICollection<rating> ratings { get; set; } = new List<rating>();

    public virtual ICollection<reading_progress> reading_progresses { get; set; } = new List<reading_progress>();

    public virtual ICollection<refresh_token> refresh_tokens { get; set; } = new List<refresh_token>();

    public virtual ICollection<story> stories { get; set; } = new List<story>();
}
