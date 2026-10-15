using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.API.DTOs
{
    public class ResultRequest
    {
        [Range(1, int.MaxValue)]
        public int EntryID { get; set; }

        [Required]
        [StringLength(50)]
        public string completiontime { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int FinishingPosition { get; set; }
    }
}
