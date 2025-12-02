using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using TravelAgencyAPI.Models;

namespace TravelAgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TravelDbContext _context;

        public AuthController(TravelDbContext context)
        {
            _context = context;
        }

        // 🔹 Register
        [HttpPost("register")]
        public async Task<IActionResult> Register(User request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email đã tồn tại!");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = HashPassword(request.Password),
                Gender = request.Gender,
                Age = request.Age,
                Avatar = request.Avatar,
                Interest = request.Interest,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!", user });
        }

        // 🔹 Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return NotFound("Email không tồn tại!");

            if (user.Password != HashPassword(request.Password))
                return BadRequest("Sai mật khẩu!");

            if (user.IsActive == false)
                return Unauthorized("Tài khoản bị khoá!");

            return Ok(new
            {
                message = "Đăng nhập thành công!",
                user = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Gender,
                    user.Age,
                    user.Avatar,
                    user.Interest,
                    user.IsActive,
                    user.CreatedAt
                }
            });
        }

        // 🔹 Hash password
        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
