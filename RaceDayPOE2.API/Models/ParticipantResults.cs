using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.API.Models
{
    public class ParticipantResults
    {
        public int resultID { get; set; }
        public required int participantID { get; set; }

        public required int entryID { get; set; }

        public required string completionTime { get; set; } = string.Empty;

        public int? finishingPosition { get; set; }
    }
}
