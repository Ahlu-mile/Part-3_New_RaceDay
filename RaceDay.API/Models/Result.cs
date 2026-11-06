using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }

        [Required]
        public int CapturedByUserId { get; set; }

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(EnrolmentId))]
        public Enrolment? Enrolment { get; set; }

        [ForeignKey(nameof(CapturedByUserId))]
        public User? CapturedByUser { get; set; }
    }
}
