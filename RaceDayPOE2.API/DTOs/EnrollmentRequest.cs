using System.ComponentModel.DataAnnotations;

namespace RaceDayPOE2.API.DTOs
{
    public class EnrollmentRequest
    {
        [Range(1, int.MaxValue)]
        public int EventID { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }
    }
}
