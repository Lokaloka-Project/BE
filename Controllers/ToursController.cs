using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyAPI.Models;
using System.Globalization;
using System.Text;

namespace TravelAgencyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToursController : ControllerBase
    {
        private readonly TravelDbContext _db;
        private readonly ILogger<ToursController> _logger;

        public ToursController(TravelDbContext db, ILogger<ToursController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: api/tours
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAll(
            [FromQuery] int? userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var q = _db.Tours
                .AsNoTracking()
                .AsQueryable();

            if (userId.HasValue)
                q = q.Where(t => t.UserId == userId.Value);

            var list = await q
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Duration,
                    t.Price,
                    t.Address,
                    t.CreatedAt,
                    t.Image,
                    Locations = t.TourLocations.OrderBy(tl => tl.DayNumber).ThenBy(tl => tl.OrderInDay).Select(tl => new
                    {
                        tl.Id,
                        tl.LocationId,
                        tl.DayNumber,
                        tl.OrderInDay,
                        tl.Note,
                        LocationName = tl.Location.Name,
                        LocationLat = tl.Location.CoordinateX,
                        LocationLng = tl.Location.CoordinateY,
                        Image = _db.LocationDetails
            .Where(ld => ld.LocationId == tl.LocationId)
            .Select(ld => ld.Image)
            .FirstOrDefault()
            ?? "https://via.placeholder.com/400x300?text=Thiếu+ảnh"
                    })
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/tours/search?city=Quang%20Ngai
        [HttpGet("search")]
        public async Task<IActionResult> SearchTours(
            [FromQuery] string city,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest(new { message = "City is required" });

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var q = _db.Tours
                .AsNoTracking()
                .Where(t => t.Address.ToLower().Contains(city.ToLower()));

            var list = await q
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Duration,
                    t.Price,
                    t.Address,
                    t.CreatedAt,
                    t.Image,

                    // ⭐ GIỐNG GetAll
                    Locations = t.TourLocations
                        .OrderBy(tl => tl.DayNumber)
                        .ThenBy(tl => tl.OrderInDay)
                        .Select(tl => new
                        {
                            tl.Id,
                            tl.LocationId,
                            tl.DayNumber,
                            tl.OrderInDay,
                            tl.Note,

                            LocationName = tl.Location.Name,
                            LocationLat = tl.Location.CoordinateX,
                            LocationLng = tl.Location.CoordinateY,

                            Image = _db.LocationDetails
                                .Where(ld => ld.LocationId == tl.LocationId)
                                .Select(ld => ld.Image)
                                .FirstOrDefault()
                                ?? "https://via.placeholder.com/400x300?text=Thiếu+ảnh"
                        })
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetTourDetail(int id)
        {
            var tour = await _db.Tours
                .Include(t => t.TourLocations)
                .ThenInclude(tl => tl.Location)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Duration,
                    t.Price,
                    t.Address,
                    t.CreatedAt,
                    Locations = t.TourLocations
                        .OrderBy(tl => tl.DayNumber)
                        .ThenBy(tl => tl.OrderInDay)
                        .Select(tl => new
                        {
                            tl.Id,
                            tl.LocationId,
                            tl.DayNumber,
                            tl.OrderInDay,
                            tl.Note,
                            LocationName = tl.Location.Name,
                            LocationLat = tl.Location.CoordinateX,
                            LocationLng = tl.Location.CoordinateY,
                            LocationDescription = tl.Location.Description,

                            // 🖼️ Lấy ảnh từ bảng LocationDetails
                            Image = _db.LocationDetails
                                .Where(ld => ld.LocationId == tl.LocationId)
                                .Select(ld => ld.Image)
                                .FirstOrDefault()
                                ?? "https://via.placeholder.com/400x300?text=Thiếu+ảnh"
                        })
                })
                .FirstOrDefaultAsync();

            if (tour == null) return NotFound();
            return Ok(tour);
        }


        // POST: api/tours
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] TourCreateRequest req)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == req.UserId);
            if (!userExists) return BadRequest("User not found.");

            var tour = new Tour
            {
                UserId = req.UserId,
                Title = req.Title,
                Description = req.Description,
                Duration = req.Duration,
                Price = req.Price,
                Address = req.Address
            };

            _db.Tours.Add(tour);
            await _db.SaveChangesAsync();

            // optionally add initial locations
            if (req.InitialLocationIds != null)
            {
                int day = 1;
                int order = 1;
                foreach (var lid in req.InitialLocationIds)
                {
                    _db.TourLocations.Add(new TourLocation
                    {
                        TourId = tour.Id,
                        LocationId = lid,
                        DayNumber = day,
                        OrderInDay = order++
                    });
                }
                await _db.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetTourDetail), new { id = tour.Id }, new { tour.Id });
        }

        // PUT: api/tours/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TourUpdateRequest req)
        {
            var tour = await _db.Tours.FindAsync(id);
            if (tour == null) return NotFound();

            tour.Title = req.Title ?? tour.Title;
            tour.Description = req.Description ?? tour.Description;
            tour.Duration = req.Duration ?? tour.Duration;
            tour.Price = req.Price ?? tour.Price;
            tour.Address = req.Address ?? tour.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/tours/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Load Tour + các location liên quan
            var tour = await _db.Tours
                .Include(t => t.TourLocations)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null)
                return NotFound();

            // Xoá toàn bộ TourLocations trước
            var locations = _db.TourLocations.Where(x => x.TourId == id);
            _db.TourLocations.RemoveRange(locations);

            // Sau đó mới xoá tour
            _db.Tours.Remove(tour);

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/tours/{id}/locations
        [HttpPost("{id:int}/locations")]
        public async Task<IActionResult> AddLocation(int id, [FromBody] AddLocationRequest req)
        {
            var tour = await _db.Tours.FindAsync(id);
            if (tour == null) return NotFound("Tour not found.");
            var location = await _db.Locations.FindAsync(req.LocationId);
            if (location == null) return BadRequest("Location not found.");

            // compute next order for specified day
            var maxOrder = await _db.TourLocations
                .Where(tl => tl.TourId == id && tl.DayNumber == req.DayNumber)
                .MaxAsync(tl => (int?)tl.OrderInDay) ?? 0;

            var tlItem = new TourLocation
            {
                TourId = id,
                LocationId = req.LocationId,
                DayNumber = req.DayNumber,
                OrderInDay = maxOrder + 1,
                Note = req.Note
            };

            _db.TourLocations.Add(tlItem);
            await _db.SaveChangesAsync();
            return Ok(new { tlItem.Id });
        }

        // DELETE: api/tours/{id}/locations/{tourLocationId}
        [HttpDelete("{id:int}/locations/{tourLocationId:int}")]
        public async Task<IActionResult> RemoveLocation(int id, int tourLocationId)
        {
            var tl = await _db.TourLocations.FirstOrDefaultAsync(x => x.Id == tourLocationId && x.TourId == id);
            if (tl == null) return NotFound();

            _db.TourLocations.Remove(tl);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/tours/{id}/reorder
        // body: [{"tourLocationId": 10, "dayNumber":1, "orderInDay":1}, ...]
        [HttpPut("{id:int}/reorder")]
        public async Task<IActionResult> Reorder(int id, [FromBody] List<ReorderRequest> newOrder)
        {
            var tour = await _db.Tours.Include(t => t.TourLocations).FirstOrDefaultAsync(t => t.Id == id);
            if (tour == null) return NotFound();

            var dict = tour.TourLocations.ToDictionary(x => x.Id);

            foreach (var item in newOrder)
            {
                if (dict.TryGetValue(item.TourLocationId, out var tl))
                {
                    tl.DayNumber = item.DayNumber;
                    tl.OrderInDay = item.OrderInDay;
                }
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/tours/share/{id}
        [HttpGet("share/{id:int}")]
        public async Task<IActionResult> Share(int id)
        {
            var tour = await _db.Tours.Include(t => t.TourLocations).ThenInclude(tl => tl.Location).FirstOrDefaultAsync(t => t.Id == id);
            if (tour == null) return NotFound();

            // simple share token (not secure) — in production you may generate signed token or persisted public copy
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"tour:{tour.Id}:{tour.CreatedAt?.ToString("o")}"));
            var shareLink = $"{Request.Scheme}://{Request.Host}/tour/shared/{token}";

            return Ok(new { shareLink });
        }

        // Request DTOs
        public class TourCreateRequest
        {
            public int UserId { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Duration { get; set; }
            public decimal? Price { get; set; }
            public string? Address { get; set; }
            public List<int>? InitialLocationIds { get; set; }
        }

        public class TourUpdateRequest
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
            public string? Duration { get; set; }
            public decimal? Price { get; set; }
            public string? Address { get; set; }
        }

        public class AddLocationRequest
        {
            public int LocationId { get; set; }
            public int DayNumber { get; set; } = 1;
            public int? OrderInDay { get; set; }
            public string? Note { get; set; }
        }

        public class ReorderRequest
        {
            public int TourLocationId { get; set; }
            public int DayNumber { get; set; }
            public int OrderInDay { get; set; }
        }
    }
}
