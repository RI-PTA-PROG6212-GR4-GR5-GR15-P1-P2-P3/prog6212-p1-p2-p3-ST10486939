namespace RaceDayPOE2.API.Models
{
    public class Event
    {
        public Guid eventID { get; set; }
        public required int organizerID { get; set; }
        public required string eventName { get; set; }
        public required string eventLocation { get; set; }
        public required string description { get; set; }
        public required string distance { get; set; }
        public required int categoryID { get; set; }
        public required string type { get; set; }
    }
}
