using System;
using System.Collections.Generic;

namespace TravelAgencyAPI.Models;

public partial class LocationDetail
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public decimal? Ticket { get; set; }

    public string? Image { get; set; }

    public string? Review { get; set; }

    public string? ServiceAround { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Location Location { get; set; } = null!;
}
