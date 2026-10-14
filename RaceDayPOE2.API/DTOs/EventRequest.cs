using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.API.DTOs
{
    public class EventRequest
    {
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EventLocation { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Distance { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set;  } = string.Empty;

        public DateTime EventDate { get; set; }
    }
}
