using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Participant")]
    public class ParticipantController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetParticipants()
        {
            return Ok("Participant API is working.");
        }
    }
}
