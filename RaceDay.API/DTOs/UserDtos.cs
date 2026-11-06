using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
    }
}
