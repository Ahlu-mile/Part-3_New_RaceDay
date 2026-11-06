using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers
{
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public EnrolmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Enters the logged-in participant into a chosen category.</summary>
        [HttpPost("api/enrolments")]
        [RequireRole(UserRoles.Participant)]
        public async Task<ActionResult<EnrolmentResponseDto>> Create(EnrolmentCreateDto dto)
        {
            var category = await _db.Categories
                .Include(c => c.Event)
                .Include(c => c.Enrolments)
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

            if (category is null) return NotFound(new { message = "Category not found." });

            var participantId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var alreadyEnrolled = await _db.Enrolments.AnyAsync(
                e => e.ParticipantId == participantId && e.CategoryId == dto.CategoryId);
            if (alreadyEnrolled)
            {
                return Conflict(new { message = "You are already enrolled in this category." });
            }

            var confirmedCount = category.Enrolments.Count(e => e.Status != EnrolmentStatus.Cancelled);
            if (confirmedCount >= category.MaxParticipants)
            {
                return BadRequest(new { message = "This category is full." });
            }

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                CategoryId = dto.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = EnrolmentStatus.Pending
            };

            _db.Enrolments.Add(enrolment);
            await _db.SaveChangesAsync();

            var full = await LoadForResponse(enrolment.EnrolmentId);
            return CreatedAtAction(nameof(GetMine), null, full);
        }

        /// <summary>Lists all enrolments for the logged-in participant.</summary>
        [HttpGet("api/enrolments/me")]
        [RequireRole(UserRoles.Participant)]
        public async Task<ActionResult<IEnumerable<EnrolmentResponseDto>>> GetMine()
        {
            var participantId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var enrolments = await _db.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .Where(e => e.ParticipantId == participantId)
                .ToListAsync();

            return Ok(enrolments.Select(ToDto));
        }

        /// <summary>Lists all enrolments for an event, for the organiser managing it.</summary>
        [HttpGet("api/events/{eventId}/enrolments")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<IEnumerable<EnrolmentResponseDto>>> GetForEvent(int eventId)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev is null) return NotFound(new { message = "Event not found." });

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (ev.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own this event." });
            }

            var enrolments = await _db.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .Where(e => e.Category!.EventId == eventId)
                .ToListAsync();

            return Ok(enrolments.Select(ToDto));
        }

        /// <summary>Updates an enrolment's status (confirm/cancel). Organiser (owner) only.</summary>
        [HttpPut("api/enrolments/{id}/status")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<EnrolmentResponseDto>> UpdateStatus(int id, EnrolmentStatusUpdateDto dto)
        {
            if (dto.Status != EnrolmentStatus.Pending && dto.Status != EnrolmentStatus.Confirmed && dto.Status != EnrolmentStatus.Cancelled)
            {
                return BadRequest(new { message = "Status must be Pending, Confirmed, or Cancelled." });
            }

            var enrolment = await _db.Enrolments
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment is null) return NotFound();

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (enrolment.Category!.Event!.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own the event this enrolment belongs to." });
            }

            enrolment.Status = dto.Status;
            if (dto.Status == EnrolmentStatus.Confirmed && string.IsNullOrEmpty(enrolment.RaceNumber))
            {
                enrolment.RaceNumber = $"R{enrolment.EnrolmentId:D4}";
            }

            await _db.SaveChangesAsync();

            var full = await LoadForResponse(enrolment.EnrolmentId);
            return Ok(full);
        }

        /// <summary>Cancels the logged-in participant's own enrolment.</summary>
        [HttpDelete("api/enrolments/{id}")]
        [RequireRole(UserRoles.Participant)]
        public async Task<IActionResult> Delete(int id)
        {
            var enrolment = await _db.Enrolments.FindAsync(id);
            if (enrolment is null) return NotFound();

            var participantId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (enrolment.ParticipantId != participantId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own this enrolment." });
            }

            _db.Enrolments.Remove(enrolment);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private async Task<EnrolmentResponseDto> LoadForResponse(int enrolmentId)
        {
            var e = await _db.Enrolments
                .Include(x => x.Participant)
                .Include(x => x.Category).ThenInclude(c => c!.Event)
                .FirstAsync(x => x.EnrolmentId == enrolmentId);

            return ToDto(e);
        }

        private static EnrolmentResponseDto ToDto(Enrolment e) => new()
        {
            EnrolmentId = e.EnrolmentId,
            ParticipantId = e.ParticipantId,
            ParticipantName = e.Participant?.FullName ?? string.Empty,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.CategoryName ?? string.Empty,
            EventId = e.Category?.EventId ?? 0,
            EventName = e.Category?.Event?.EventName ?? string.Empty,
            EnrolmentDate = e.EnrolmentDate,
            Status = e.Status,
            RaceNumber = e.RaceNumber
        };
    }
}
