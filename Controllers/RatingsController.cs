using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyAPI.Models;
using System.Globalization;
using System.Text;

namespace TravelAgencyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly TravelDbContext _db;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(TravelDbContext db, ILogger<RatingsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: api/ratings?locationId=1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? locationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var q = _db.Ratings.AsNoTracking()
                .Include(r => r.Images)
                .AsQueryable();

            if (locationId.HasValue) q = q.Where(r => r.LocationId == locationId.Value);

            var list = await q
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    r.LocationId,
                    RatingValue = r.RatingValue,
                    r.Comment,
                    r.CreatedAt,
                    Images = r.Images.Select(i => i.Image1)
                })
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/ratings
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] RatingCreateRequest req)
        {
            // Basic validation
            if (req.RatingValue < 0 || req.RatingValue > 5) return BadRequest("RatingValue must be between 0 and 5.");
            var userExists = await _db.Users.AnyAsync(u => u.Id == req.UserId);
            if (!userExists) return BadRequest("User not found.");
            var locExists = await _db.Locations.AnyAsync(l => l.Id == req.LocationId);
            if (!locExists) return BadRequest("Location not found.");

            var rating = new Rating
            {
                UserId = req.UserId,
                LocationId = req.LocationId,
                RatingValue = req.RatingValue,
                Comment = req.Comment,
            };

            _db.Ratings.Add(rating);
            await _db.SaveChangesAsync();

            // Save images if provided (simple url strings)
            if (req.Images != null && req.Images.Any())
            {
                foreach (var img in req.Images)
                {
                    _db.Images.Add(new Image { RatingId = rating.Id, Image1 = img });
                }
                await _db.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetAll), new { locationId = rating.LocationId }, new { rating.Id });
        }

        // DELETE: api/ratings/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _db.Ratings.Include(r => r.Images).FirstOrDefaultAsync(r => r.Id == id);
            if (rating == null) return NotFound();

            _db.Images.RemoveRange(rating.Images);
            _db.Ratings.Remove(rating);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        public class RatingCreateRequest
        {
            public int UserId { get; set; }
            public int LocationId { get; set; }
            public decimal RatingValue { get; set; } // 0..5
            public string? Comment { get; set; }
            public List<string>? Images { get; set; } // URL strings or base64 — recommend uploading separately
        }
    }
}
