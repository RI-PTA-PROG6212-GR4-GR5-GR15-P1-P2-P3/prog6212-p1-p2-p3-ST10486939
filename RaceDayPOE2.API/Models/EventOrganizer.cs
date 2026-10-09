namespace RaceDayPOE2.API.Models
{
    public class EventOrganizer
    {
        public Guid organizerID { get; set; }
        public required string name { get; set; }
        public required int roleID { get; set; }
        public required string emailAddress { get; set; }
        public required string password { get; set; }
    }
}
