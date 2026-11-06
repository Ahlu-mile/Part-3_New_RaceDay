using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal DistanceKM { get; set; }

        [Required]
        public int MaxParticipants { get; set; } = 500;

        [Required, Column(TypeName = "decimal(8,2)")]
        public decimal EntryFee { get; set; } = 0.00m;

        // Navigation properties
        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
