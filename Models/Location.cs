using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;

namespace TravelAgencyAPI.Models;

public partial class Location
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? City { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Type { get; set; }

    public decimal? CoordinateX { get; set; }

    public decimal? CoordinateY { get; set; }

    public TimeOnly? OpeningTime { get; set; }

    public TimeOnly? ClosingTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Language { get; set; }

    public virtual ICollection<LocationDetail> LocationDetails { get; set; } = new List<LocationDetail>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<TourLocation> TourLocations { get; set; } = new List<TourLocation>();

    public ICollection<LocationTranslation> Translations { get; set; } = new List<LocationTranslation>();
}
