using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Models;
using RaceDay.API.Security;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public EventsController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Lists all events. Public - no login required.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetAll(
            [FromQuery] string? eventType, [FromQuery] string? location)
        {
            var query = _db.Events.Include(e => e.Organiser).AsQueryable();

            if (!string.IsNullOrWhiteSpace(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(e => e.Location.Contains(location));

            var events = await query.OrderBy(e => e.EventDate).ToListAsync();

            return Ok(events.Select(ToDto));
        }

        /// <summary>Lists events owned by the logged-in organiser.</summary>
        [HttpGet("mine")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetMine()
        {
            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            var events = await _db.Events
                .Include(e => e.Organiser)
                .Where(e => e.OrganiserId == organiserId)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(events.Select(ToDto));
        }

        /// <summary>Returns full details for a single event. Public.</summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetById(int id)
        {
            var ev = await _db.Events.Include(e => e.Organiser)
                                      .FirstOrDefaultAsync(e => e.EventId == id);
            if (ev is null) return NotFound();

            return Ok(ToDto(ev));
        }

        /// <summary>Returns route/course info for an event (race-day prep). Public.</summary>
        [HttpGet("{id}/route")]
        public async Task<ActionResult<IEnumerable<object>>> GetRoutes(int id)
        {
            var eventExists = await _db.Events.AnyAsync(e => e.EventId == id);
            if (!eventExists) return NotFound();

            var routes = await _db.RouteInfos
                .Where(r => r.EventId == id)
                .Select(r => new
                {
                    r.RouteId,
                    r.RouteName,
                    r.DistanceKM,
                    r.ElevationGainM,
                    r.MapURL,
                    r.StartingPoint
                })
                .ToListAsync();

            return Ok(routes);
        }

        /// <summary>Creates a new event. Organiser only.</summary>
        [HttpPost]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<EventResponseDto>> Create(EventCreateDto dto)
        {
            if (dto.EventType != EventTypes.Run && dto.EventType != EventTypes.Walk && dto.EventType != EventTypes.Cycle)
            {
                return BadRequest(new { message = "EventType must be Run, Walk, or Cycle." });
            }

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;

            var ev = new Event
            {
                OrganiserId = organiserId,
                EventName = dto.EventName,
                Description = dto.Description,
                EventType = dto.EventType,
                EventDate = dto.EventDate,
                Location = dto.Location,
                StartTime = dto.StartTime,
                CreatedAt = DateTime.UtcNow
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            await _db.Entry(ev).Reference(e => e.Organiser).LoadAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ToDto(ev));
        }

        /// <summary>Updates an event owned by the logged-in organiser.</summary>
        [HttpPut("{id}")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<ActionResult<EventResponseDto>> Update(int id, EventUpdateDto dto)
        {
            if (dto.EventType != EventTypes.Run && dto.EventType != EventTypes.Walk && dto.EventType != EventTypes.Cycle)
            {
                return BadRequest(new { message = "EventType must be Run, Walk, or Cycle." });
            }

            var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventId == id);
            if (ev is null) return NotFound();

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (ev.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own this event." });
            }

            ev.EventName = dto.EventName;
            ev.Description = dto.Description;
            ev.EventType = dto.EventType;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.StartTime = dto.StartTime;

            await _db.SaveChangesAsync();
            return Ok(ToDto(ev));
        }

        /// <summary>Deletes an event owned by the logged-in organiser.</summary>
        [HttpDelete("{id}")]
        [RequireRole(UserRoles.Organiser)]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev is null) return NotFound();

            var organiserId = HttpContext.Session.GetInt32(SessionKeys.UserId)!.Value;
            if (ev.OrganiserId != organiserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "You do not own this event." });
            }

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static EventResponseDto ToDto(Event e) => new()
        {
            EventId = e.EventId,
            OrganiserId = e.OrganiserId,
            OrganiserName = e.Organiser?.FullName ?? string.Empty,
            EventName = e.EventName,
            Description = e.Description,
            EventType = e.EventType,
            EventDate = e.EventDate,
            Location = e.Location,
            BannerImageUrl = e.BannerImageUrl,
            StartTime = e.StartTime,
            CreatedAt = e.CreatedAt
        };
    }
}
