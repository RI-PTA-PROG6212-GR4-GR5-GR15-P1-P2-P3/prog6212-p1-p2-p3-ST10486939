using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetParticipants()
        {
            return Ok("Participant API is working.");
        }
    }
}
