using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

        /// <summary>Pending, Confirmed, or Cancelled.</summary>
        [Required, MaxLength(20)]
        public string Status { get; set; } = EnrolmentStatus.Pending;

        [MaxLength(10)]
        public string? RaceNumber { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ParticipantId))]
        public User? Participant { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public Result? Result { get; set; }
    }

    public static class EnrolmentStatus
    {
        public const string Pending = "Pending";
        public const string Confirmed = "Confirmed";
        public const string Cancelled = "Cancelled";
    }
}
