namespace RaceDayPOE2.MVC.Models.ViewModels
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public int OrganizerID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string EventLocation { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
    }
}
