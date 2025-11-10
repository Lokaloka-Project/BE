using System;
using System.Collections.Generic;

namespace TravelAgencyAPI.Models;

public partial class TourLocation
{
    public int Id { get; set; }

    public int TourId { get; set; }

    public int LocationId { get; set; }

    public int? DayNumber { get; set; }

    public int? OrderInDay { get; set; }

    public string? Note { get; set; }

    public virtual Location Location { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;
}
