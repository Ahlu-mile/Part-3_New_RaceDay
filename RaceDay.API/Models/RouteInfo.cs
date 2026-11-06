using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class RouteInfo
    {
        public int RouteId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required, MaxLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKM { get; set; }

        public int? ElevationGainM { get; set; }

        [MaxLength(255)]
        public string? MapURL { get; set; }

        [Required, MaxLength(150)]
        public string StartingPoint { get; set; } = string.Empty;

        // Navigation property
        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }
    }
}
