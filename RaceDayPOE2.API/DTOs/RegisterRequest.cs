namespace RaceDayPOE2.API.DTOs
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? Age { get; set; }
    }
}
