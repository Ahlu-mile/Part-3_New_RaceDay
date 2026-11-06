using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Returns the profile of the currently logged-in user.</summary>
        [HttpGet("me")]
        [HttpGet("/api/profile/me")]
        [RequireAuth]
        public async Task<ActionResult<UserProfileDto>> GetMe()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var user = await _db.Users.FindAsync(userId);

            if (user is null) return NotFound();

            return Ok(new UserProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>Updates the logged-in user's own profile details.</summary>
        [HttpPut("me")]
        [HttpPut("/api/profile/me")]
        [RequireAuth]
        public async Task<ActionResult<UserProfileDto>> UpdateMe(UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var user = await _db.Users.FindAsync(userId);

            if (user is null) return NotFound();

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            await _db.SaveChangesAsync();

            return Ok(new UserProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>Returns a specific user's public profile (e.g. organiser contact info).</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserProfileDto>> GetById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null) return NotFound();

            return Ok(new UserProfileDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt
            });
        }
    }
}
