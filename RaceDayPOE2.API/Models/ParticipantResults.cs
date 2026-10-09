namespace RaceDayPOE2.API.Models
{
    public class ParticipantResults
    {
        public required int participantID {  get; set; }
        public required int entryID { get; set; }
        public string? completionTime { get; set; }
    }
}
