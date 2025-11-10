using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgencyAPI.Models
{
    public class LocationTranslation
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Location")]
        public int LocationId { get; set; }  // ✅ int (khớp với Location.Id)

        [Required]
        [MaxLength(10)]
        public string Language { get; set; } = "en";

        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }

        public Location? Location { get; set; }
    }
}
