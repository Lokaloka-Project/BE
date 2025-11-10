namespace TravelAgencyAPI.Models.Dto
{
    public class LocationListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Type { get; set; }
        public decimal? Latitude { get; set; }   // CoordinateX
        public decimal? Longitude { get; set; }  // CoordinateY
        public double? Rating { get; set; }
        public string? Image { get; set; }
        public decimal? Ticket { get; set; }
        public double? DistanceKm { get; set; } // optional
    }

    public class LocationDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? OpeningHours { get; set; }
        public string? ClosingHours { get; set; }
        public double? Rating { get; set; }
        public IEnumerable<LocationDetailItemDto> Details { get; set; } = new List<LocationDetailItemDto>();
        public IEnumerable<RatingDto> Ratings { get; set; } = new List<RatingDto>();
    }

    public class LocationDetailItemDto
    {
        public int Id { get; set; }
        public decimal? Ticket { get; set; }
        public string? Image { get; set; }
        public string? ServiceAround { get; set; }
        public string? Review { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class RatingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal? RatingValue { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public IEnumerable<string>? Images { get; set; }
    }
}
