using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers
{
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ResultsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Captures a finish result against a participant's enrolment. Organiser only.</summary>
        [HttpPost("api/results")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<ResultResponseDto>> Create(ResultCreateDto dto)
        {
            var enrolment = await _db.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category).ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == dto.EnrolmentId);

            if (enrolment is null) return NotFound(new { message = "Enrolment not found." });

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (enrolment.Category!.Event!.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own the event this enrolment belongs to." });
            }

            var alreadyCaptured = await _db.Results.AnyAsync(r => r.EnrolmentId == dto.EnrolmentId);
            if (alreadyCaptured)
            {
                return Conflict(new { message = "A result has already been captured for this enrolment." });
            }

            var result = new Result
            {
                EnrolmentId = dto.EnrolmentId,
                FinishTime = dto.FinishTime,
                OverallPosition = dto.OverallPosition,
                CategoryPosition = dto.CategoryPosition,
                CapturedByUserId = organiserId,
                CapturedAt = DateTime.UtcNow
            };

            _db.Results.Add(result);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetForEvent),
                new { eventId = enrolment.Category.EventId },
                ToDto(result, enrolment));
        }

        /// <summary>Returns the logged-in participant's personal performance history.</summary>
        [HttpGet("api/results/me")]
        [RequireRole(UserRoles.Participant)]
        public async Task<ActionResult<IEnumerable<ResultResponseDto>>> GetMine()
        {
            var participantId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var results = await _db.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
                .Where(r => r.Enrolment!.ParticipantId == participantId)
                .ToListAsync();

            return Ok(results.Select(r => ToDto(r, r.Enrolment!)));
        }

        /// <summary>Returns the full results list (leaderboard) for an event. Public.</summary>
        [HttpGet("api/events/{eventId}/results")]
        public async Task<ActionResult<IEnumerable<ResultResponseDto>>> GetForEvent(int eventId)
        {
            var eventExists = await _db.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists) return NotFound(new { message = "Event not found." });

            var results = await _db.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
                .Where(r => r.Enrolment!.Category!.EventId == eventId)
                .OrderBy(r => r.OverallPosition)
                .ToListAsync();

            return Ok(results.Select(r => ToDto(r, r.Enrolment!)));
        }

        /// <summary>Corrects a previously captured result. Organiser (owner) only.</summary>
        [HttpPut("api/results/{id}")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<ResultResponseDto>> Update(int id, ResultUpdateDto dto)
        {
            var result = await _db.Results
                .Include(r => r.Enrolment).ThenInclude(e => e!.Category).ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result is null) return NotFound();

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (result.Enrolment!.Category!.Event!.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own the event this result belongs to." });
            }

            result.FinishTime = dto.FinishTime;
            result.OverallPosition = dto.OverallPosition;
            result.CategoryPosition = dto.CategoryPosition;

            await _db.SaveChangesAsync();
            return Ok(ToDto(result, result.Enrolment));
        }

        private static ResultResponseDto ToDto(Result r, Enrolment e) => new()
        {
            ResultId = r.ResultId,
            EnrolmentId = r.EnrolmentId,
            ParticipantName = e.Participant?.FullName ?? string.Empty,
            EventName = e.Category?.Event?.EventName ?? string.Empty,
            CategoryName = e.Category?.CategoryName ?? string.Empty,
            FinishTime = r.FinishTime,
            OverallPosition = r.OverallPosition,
            CategoryPosition = r.CategoryPosition,
            CapturedAt = r.CapturedAt
        };
    }
}
