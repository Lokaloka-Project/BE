using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyAPI.Models;
using TravelAgencyAPI.Models.Dto;

namespace TravelAgencyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly TravelDbContext _db;
        private readonly ILogger<LocationsController> _logger;

        public LocationsController(TravelDbContext db, ILogger<LocationsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ✅ GET all locations (with image from LocationDetails)
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _db.Locations
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.City,
                    l.Description,
                    l.Type,
                    l.CoordinateX,
                    l.CoordinateY,
                    l.OpeningTime,
                    l.ClosingTime,
                    // 👇 Lấy hình đầu tiên từ bảng LocationDetails nếu có
                    Image = _db.LocationDetails
                        .Where(d => d.LocationId == l.Id)
                        .Select(d => d.Image)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(locations);
        }

        // ✅ GET: /api/Locations?lang=en&keyword=beach&city=quang ngai
        [HttpGet]
        public async Task<IActionResult> GetLocationsByKey(
            [FromQuery] string? keyword,
            [FromQuery] string? city,
            [FromQuery] string? type,
            [FromQuery] string lang = "vi")
        {
            var query = _db.Locations
                .Include(l => l.Translations)
                .AsQueryable();

            // Lọc cơ bản (chỉ tiếng Việt)
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(l => l.Name.Contains(keyword) || (l.Description ?? "").Contains(keyword));

            if (!string.IsNullOrEmpty(city))
                query = query.Where(l => l.City.Contains(city));

            if (!string.IsNullOrEmpty(type))
                query = query.Where(l => l.Type.Contains(type));

            var list = await query
                .Select(l => new
                {
                    l.Id,
                    Name = lang == "en"
                        ? (l.Translations.FirstOrDefault(t => t.Language == "en").Name ?? l.Name)
                        : l.Name,
                    City = lang == "en"
                        ? (l.Translations.FirstOrDefault(t => t.Language == "en").City ?? l.City)
                        : l.City,
                    Description = lang == "en"
                        ? (l.Translations.FirstOrDefault(t => t.Language == "en").Description ?? l.Description)
                        : l.Description,
                    Type = lang == "en"
                        ? (l.Translations.FirstOrDefault(t => t.Language == "en").Type ?? l.Type)
                        : l.Type
                })
                .ToListAsync();

            return Ok(list);
        }

        // ✅ GET by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocation(long id, [FromQuery] string lang = "vi")
        {
            var loc = await _db.Locations
                .Include(l => l.Translations)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loc == null) return NotFound();

            var trans = loc.Translations.FirstOrDefault(t => t.Language == lang);

            return Ok(new
            {
                loc.Id,
                Name = trans?.Name ?? loc.Name,
                City = trans?.City ?? loc.City,
                Description = trans?.Description ?? loc.Description,
                Type = trans?.Type ?? loc.Type
            });
        }

        // ✅ POST: Tạo mới (có thể gửi luôn bản dịch)
        [HttpPost]
        public async Task<IActionResult> CreateLocation([FromBody] Location location)
        {
            _db.Locations.Add(location);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, location);
        }

        // ✅ PUT: Cập nhật location gốc
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(long id, [FromBody] Location update)
        {
            var loc = await _db.Locations.FindAsync(id);
            if (loc == null) return NotFound();

            loc.Name = update.Name;
            loc.City = update.City;
            loc.Description = update.Description;
            loc.Type = update.Type;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ✅ PUT: Cập nhật bản dịch
        [HttpPut("{id}/translations/{lang}")]
        public async Task<IActionResult> UpdateTranslation(int id, string lang, [FromBody] LocationTranslation model)
        {
            var translation = await _db.LocationTranslations
                .FirstOrDefaultAsync(t => t.LocationId == id && t.Language == lang);

            if (translation == null)
            {
                model.LocationId = id;
                model.Language = lang;
                _db.LocationTranslations.Add(model);
            }
            else
            {
                translation.Name = model.Name;
                translation.City = model.City;
                translation.Description = model.Description;
                translation.Type = model.Type;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(long id)
        {
            var loc = await _db.Locations
                .Include(l => l.Translations)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loc == null) return NotFound();

            _db.Locations.Remove(loc);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("ByRating")]
        public IActionResult ByRating(int minReviews = 1, int top = 5)
        {
            var result = _db.Locations
                .Include(l => l.Ratings)
                    .ThenInclude(r => r.Images) // ✅ Load luôn ảnh của từng rating
                .Where(l => l.Ratings.Any())
                .Select(l => new
                {
                    l.Id,
                    l.Name,
                    l.City,
                    l.Description,
                    // ✅ Lấy ảnh đầu tiên trong tất cả các rating của Location
                    Image = l.Ratings
                        .SelectMany(r => r.Images)
                        .Select(img => img.Image1)
                        .FirstOrDefault(),
                    AverageRating = Math.Round(l.Ratings.Average(r => (double)r.RatingValue), 1),
                    RatingCount = l.Ratings.Count()
                })
                .OrderByDescending(l => l.AverageRating)
                .Take(top)
                .ToList();

            return Ok(result);
        }

    }
}
