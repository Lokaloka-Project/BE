using System;
using System.Collections.Generic;

namespace TravelAgencyAPI.Models;

public partial class Tour
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Duration { get; set; }

    public bool? Status { get; set; }

    public decimal? Price { get; set; }

    public string? Address { get; set; }
    public string? Image { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<TourLocation> TourLocations { get; set; } = new List<TourLocation>();

    public virtual User User { get; set; } = null!;
}
