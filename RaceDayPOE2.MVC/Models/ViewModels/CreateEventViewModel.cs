using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.MVC.Models.ViewModels
{
    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event location is required.")]
        [StringLength(50)]
        public string EventLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required.")]
        [StringLength(50)]
        public string Distance { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Event type is required.")]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        public DateTime? EventDate { get; set; }

        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }
}
