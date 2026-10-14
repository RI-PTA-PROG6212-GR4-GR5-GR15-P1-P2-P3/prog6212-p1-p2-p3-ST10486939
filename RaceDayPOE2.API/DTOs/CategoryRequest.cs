using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.API.DTOs
{
    public class CategoryRequest
    {
        [Required]
        [StringLength(50)]
        public string Description { get; set; } = string.Empty;
    }
}
