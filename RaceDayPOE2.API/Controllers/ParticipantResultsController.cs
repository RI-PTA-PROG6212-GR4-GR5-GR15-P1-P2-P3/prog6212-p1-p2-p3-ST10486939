using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantResultsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetParticipantResults()
        {
            return Ok("ParticipantResults API is working.");
        }
    }
}
