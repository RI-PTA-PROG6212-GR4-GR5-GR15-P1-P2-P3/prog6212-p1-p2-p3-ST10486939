namespace RaceDayPOE2.MVC.Models.ViewModels
{
    public class MyEnrollmentsViewModel
    {
        public int EntryID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; } = string.Empty;

        public int CategoryID { get; set; }

        public string CategoryDescription { get; set; } = string.Empty;
    }
}