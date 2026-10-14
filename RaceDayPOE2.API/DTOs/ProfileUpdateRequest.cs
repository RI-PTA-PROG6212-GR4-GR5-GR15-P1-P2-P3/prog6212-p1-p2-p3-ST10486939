using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.API.DTOs
{
    public class ProfileUpdateRequest
    {
        public string? Name { get; set; }

        [EmailAddress]
        public string? EmailAddress { get; set; }

        //only participants
        public int? Age { get; set; }

        [MinLength(8)]
        public string? NewPassword { get; set; }
    }
}
