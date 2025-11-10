using System;
using System.Collections.Generic;

namespace TravelAgencyAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool? Gender { get; set; }

    public int? Age { get; set; }

    public string? Avatar { get; set; }

    public string? Interest { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
