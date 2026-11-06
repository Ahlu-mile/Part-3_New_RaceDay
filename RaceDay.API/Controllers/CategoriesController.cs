using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers
{
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public CategoriesController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Lists all categories for a specific event. Public.</summary>
        [HttpGet("api/events/{eventId}/categories")]
        public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> GetForEvent(int eventId)
        {
            var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists) return NotFound(new { message = "Event not found." });

            var categories = await _db.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();

            return Ok(categories.Select(ToDto));
        }

        /// <summary>Adds a new category to an event owned by the logged-in organiser.</summary>
        [HttpPost("api/events/{eventId}/categories")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<CategoryResponseDto>> Create(int eventId, CategoryCreateDto dto)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev is null) return NotFound(new { message = "Event not found." });

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (ev.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own this event." });
            }

            var category = new Category
            {
                EventId = eventId,
                CategoryName = dto.CategoryName,
                DistanceKM = dto.DistanceKM,
                MaxParticipants = dto.MaxParticipants,
                EntryFee = dto.EntryFee
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetForEvent), new { eventId }, ToDto(category));
        }

        /// <summary>Updates a category's details. Organiser (owner) only.</summary>
        [HttpPut("api/categories/{id}")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<CategoryResponseDto>> Update(int id, CategoryUpdateDto dto)
        {
            var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category is null) return NotFound();

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (category.Event!.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own the event this category belongs to." });
            }

            category.CategoryName = dto.CategoryName;
            category.DistanceKM = dto.DistanceKM;
            category.MaxParticipants = dto.MaxParticipants;
            category.EntryFee = dto.EntryFee;

            await _db.SaveChangesAsync();
            return Ok(ToDto(category));
        }

        /// <summary>Removes a category from an event. Organiser (owner) only.</summary>
        [HttpDelete("api/categories/{id}")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category is null) return NotFound();

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (category.Event!.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own the event this category belongs to." });
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static CategoryResponseDto ToDto(Category c) => new()
        {
            CategoryId = c.CategoryId,
            EventId = c.EventId,
            CategoryName = c.CategoryName,
            DistanceKM = c.DistanceKM,
            MaxParticipants = c.MaxParticipants,
            EntryFee = c.EntryFee
        };
    }
}
