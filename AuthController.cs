using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalAppAPI; 
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace MedicalAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("يرجى إدخال اسم المستخدم وكلمة المرور.");
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.Password, @"^[ -~]+$"))
            {
                return Unauthorized("خطأ: يرجى كتابة كلمة المرور باللغة الإنجليزية.");
            }

            var searchUsername = request.Username.Trim().ToLower();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == searchUsername);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role) 
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new { 
                token = new JwtSecurityTokenHandler().WriteToken(token),
                role = user.Role 
            });
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var currentUser = User.FindFirst(ClaimTypes.Name)?.Value;
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Username == currentUser);
            
            if (admin == null || admin.Role != "Admin")
            {
                return Unauthorized("عذراً، أنت لا تملك صلاحية إضافة مستخدمين!");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User 
            { 
                Username = request.Username, 
                PasswordHash = hashedPassword,
                Role = "Doctor"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("تمت إضافة المستخدم بنجاح.");
        }

        [HttpDelete("delete-doctor/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(string username)
        {
            var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            
            if (doctor == null) return NotFound("الطبيب غير موجود.");
            
            if (doctor.Role == "Admin") return BadRequest("لا يمكن حذف الأدمن الرئيسي.");

            _context.Users.Remove(doctor);
            await _context.SaveChangesAsync();

            return Ok("تم حذف الطبيب بنجاح.");
        }

       
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
    }
    

    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}