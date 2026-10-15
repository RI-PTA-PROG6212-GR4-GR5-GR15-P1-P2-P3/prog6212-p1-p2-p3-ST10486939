using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDayPOE2.API.Models
{
    public class Entries
    {
        [Key]
        public int entryID { get; set; }

        [Required]
        public int eventID { get; set; }

        [Required]
        public int participantID { get; set; }

        [Required]
        public int categoryID { get; set; }

        [Required]
        public Event? Event { get; set; }

        [Required]
        public Participant? Participant { get; set; }

        [ForeignKey(nameof(categoryID))]
        public Category? Category { get; set; }
    }
}
