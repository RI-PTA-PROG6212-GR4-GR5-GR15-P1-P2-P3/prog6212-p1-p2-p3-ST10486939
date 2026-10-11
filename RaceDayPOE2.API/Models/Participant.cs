namespace RaceDayPOE2.API.Models
{
    public class Participant
    {
        public int participantID { get; set; }
        public required string name { get; set; }
        public required int age { get; set; }
        public required int roleID { get; set; }
        public required string emailAddress { get; set; }
        public string? password { get; set; }
    }
}
