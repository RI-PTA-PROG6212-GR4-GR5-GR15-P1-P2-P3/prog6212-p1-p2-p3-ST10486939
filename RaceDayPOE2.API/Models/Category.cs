using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.API.Models
{
    public class Category
    {
        public int categoryID {  get; set; }

        [Required]
        [StringLength(50)]
        public string description { get; set; } = string.Empty;
    }
}
