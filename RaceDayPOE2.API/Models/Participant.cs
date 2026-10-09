namespace RaceDayPOE2.API.Models
{
    public class Participant
    {
        public Guid participantID { get; set; }
        public required string name { get; set; }
        public required int age { get; set; }
        public required int roleID { get; set; }
        public required string emailAddress { get; set; }
    }
}
