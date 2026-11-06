using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class ResultCreateDto
    {
        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }
    }

    public class ResultUpdateDto
    {
        [Required]
        public TimeSpan FinishTime { get; set; }

        public int? OverallPosition { get; set; }

        public int? CategoryPosition { get; set; }
    }

    public class ResultResponseDto
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int? OverallPosition { get; set; }
        public int? CategoryPosition { get; set; }
        public DateTime CapturedAt { get; set; }
    }
}
