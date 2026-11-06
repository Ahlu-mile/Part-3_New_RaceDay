using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class EventCreateDto
    {
        [Required, MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>Run, Walk, or Cycle.</summary>
        [Required]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public TimeSpan StartTime { get; set; }
    }

    public class EventUpdateDto : EventCreateDto { }

    public class EventResponseDto
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string OrganiserName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
