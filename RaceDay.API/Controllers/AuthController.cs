using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Security;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AuthController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Registers a new user as either an Organiser or a Participant.
        /// The role is supplied explicitly in the request body and validated
        /// against the two allowed values - this is how role selection is
        /// managed at registration time.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            if (dto.Role != UserRoles.Organiser && dto.Role != UserRoles.Participant)
            {
                return BadRequest(new { message = "Role must be either 'Organiser' or 'Participant'." });
            }

            var emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailExists)
            {
                return Conflict(new { message = "A user with this email is already registered." });
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Role = dto.Role,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var response = new AuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return CreatedAtAction(nameof(Register), new { id = user.UserId }, response);
        }

        /// <summary>
        /// Authenticates a user and starts a server-side session. The
        /// session stores the user's ID and role, which every subsequent
        /// request on this HTTP session relies on for authentication and
        /// role-based access control (see RequireAuthAttribute / RequireRoleAttribute).
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user is null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
            HttpContext.Session.SetString(SessionKeys.Role, user.Role);
            HttpContext.Session.SetString(SessionKeys.FullName, user.FullName);

            var response = new AuthResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(response);
        }

        /// <summary>Clears the current session (logs the user out).</summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out successfully." });
        }
    }
}
