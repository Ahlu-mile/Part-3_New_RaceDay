using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Either "Organiser" or "Participant".</summary>
        [Required, MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(1000)]
        public string? ProfileImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
        public ICollection<Result> CapturedResults { get; set; } = new List<Result>();
    }

    public static class UserRoles
    {
        public const string Organiser = "Organiser";
        public const string Participant = "Participant";
    }
}
