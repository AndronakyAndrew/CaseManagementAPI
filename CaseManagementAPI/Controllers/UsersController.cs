using CaseManagementAPI.Contracts;
using CaseManagementAPI.Data;
using CaseManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CaseManagementAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _db;

        public UsersController(AppDBContext db)
        {
            _db = db;
        }

        //List all users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var users = await _db.Users
                .Where(u => u.TenantId == tenantId)
                .Select(u => new { u.UserId, u.Name, u.Email, u.Role })
                .ToListAsync();

            return Ok(users);
        }

        //Create a new user (admin only)
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var user = new User
            {
                TenantId = tenantId,
                Name = request.Username,
                Email = request.Email,
                Role = request.Role
            };
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return Ok(new {user.UserId, user.Name, user.Email, user.Role});
        }

        //Update user (admin only)
        [Authorize(Roles = "admin")]
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.TenantId == tenantId);
            if (user == null)
                return NotFound();

            user.Email = request.Email ?? user.Email;
            user.Role = request.Role ?? user.Role;

            if(!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _db.SaveChangesAsync();
            return Ok(new { user.UserId, user.Name, user.Email, user.Role });
        }

        //Delete user (admin only)
        [Authorize(Roles = "admin")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var tenantId = Guid.Parse(User.FindFirst("TenantId")?.Value);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.TenantId == tenantId);
            if (user == null)
                return NotFound("Пользователь не существует.");

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Ok("Пользователь удален успешно!");
        }
    }
}
