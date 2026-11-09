using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.MVC.Models.ViewModels
{
    public class EnrollmentViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }
    }
}