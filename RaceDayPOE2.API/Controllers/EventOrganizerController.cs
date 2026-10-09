using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventOrganizerController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetEventOrganizers()
        {
            return Ok("EventOrganizer API is working.");
        }
    }
}
