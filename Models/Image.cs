using System;
using System.Collections.Generic;

namespace TravelAgencyAPI.Models;

public partial class Image
{
    public int Id { get; set; }

    public int? RatingId { get; set; }

    public string? Image1 { get; set; }

    public virtual Rating? Rating { get; set; }
}
