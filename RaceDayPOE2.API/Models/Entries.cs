namespace RaceDayPOE2.API.Models
{
    public class Entries
    {
        public int entryID { get; set; }
        public required int eventID { get; set; }
        public required int participantID { get; set; }
    }
}
