using System;
using System.Collections.Generic;

namespace TravelAgencyAPI.Models;

public partial class Rating
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int LocationId { get; set; }

    public decimal? RatingValue { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual Location Location { get; set; } = null!;

    public virtual User User { get; set; } = null!;
    public decimal Score { get; internal set; }
}
