using CaseManagementAPI.Contracts;
using CaseManagementAPI.Data;
using CaseManagementAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CaseManagementAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDBContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Invalid registration request.");
            }

            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Name == request.Username || u.Email == request.Email);
            if (existingUser != null)
            {
                return Conflict("User already exists.");
            }

            var user = new User
            {
                Name = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Пользователь",
                TenantId = Guid.NewGuid()
            };

            await _db.Tenants.AddAsync(new Tenant { TenantId = user.TenantId, Name = $"{request.Username}'s firm" });
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return Ok($"User successfully registered.With role: {user.Role}");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == request.Username);

            if(user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Wrong username or password");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("TenantId", user.TenantId.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
