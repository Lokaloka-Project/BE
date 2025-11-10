using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelAgencyAPI.Models;
using System.Globalization;
using System.Text;

namespace TravelAgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public TripsController(TravelDbContext context)
        {
            _context = context;
        }

        // POST: api/trips
        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] Tour tour)
        {
            tour.CreatedAt = DateTime.Now;
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Tạo lịch trình thành công!", tour.Id });
        }

        // PUT: api/trips/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(int id, [FromBody] Tour tour)
        {
            var existing = await _context.Tours.Include(t => t.TourLocations).FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return NotFound();

            existing.Title = tour.Title;
            existing.Description = tour.Description;
            existing.Price = tour.Price;
            existing.Duration = tour.Duration;

            // cập nhật các điểm đến
            _context.TourLocations.RemoveRange(existing.TourLocations);
            existing.TourLocations = tour.TourLocations;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật lịch trình thành công!" });
        }

        // GET: api/trips/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripDetail(int id)
        {
            var trip = await _context.Tours
                .Include(t => t.User)
                .Include(t => t.TourLocations)
                    .ThenInclude(tl => tl.Location)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null) return NotFound();

            return Ok(new
            {
                trip.Id,
                trip.Title,
                trip.Description,
                trip.Duration,
                trip.Price,
                User = trip.User.Name,
                Locations = trip.TourLocations
                    .OrderBy(tl => tl.DayNumber)
                    .ThenBy(tl => tl.OrderInDay)
                    .Select(tl => new
                    {
                        tl.DayNumber,
                        tl.OrderInDay,
                        tl.Note,
                        LocationName = tl.Location.Name,
                        City = tl.Location.City,
                        tl.Location.CoordinateX,
                        tl.Location.CoordinateY
                    })
            });
        }

        // POST: api/trips/share/{id}
        [HttpPost("share/{id}")]
        public IActionResult ShareTrip(int id)
        {
            string shareUrl = $"https://travelapp.vn/trip/{id}";
            return Ok(new { message = "Link chia sẻ của bạn:", url = shareUrl });
        }
    }
}
