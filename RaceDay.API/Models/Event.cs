using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required, MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>Run, Walk, or Cycle.</summary>
        [Required, MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required, MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? BannerImageUrl { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(OrganiserId))]
        public User? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<RouteInfo> Routes { get; set; } = new List<RouteInfo>();
    }

    public static class EventTypes
    {
        public const string Run = "Run";
        public const string Walk = "Walk";
        public const string Cycle = "Cycle";
    }
}
