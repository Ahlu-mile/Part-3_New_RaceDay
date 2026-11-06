using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CategoryCreateDto
    {
        [Required, MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required, Range(0.1, 1000)]
        public decimal DistanceKM { get; set; }

        [Range(1, 100000)]
        public int MaxParticipants { get; set; } = 500;

        [Range(0, 100000)]
        public decimal EntryFee { get; set; } = 0.00m;
    }

    public class CategoryUpdateDto : CategoryCreateDto { }

    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal DistanceKM { get; set; }
        public int MaxParticipants { get; set; }
        public decimal EntryFee { get; set; }
    }
}
