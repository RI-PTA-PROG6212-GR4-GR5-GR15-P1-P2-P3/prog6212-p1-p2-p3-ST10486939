using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Organizer")]
    public class EventController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetEvents()
        {
            return Ok("Events API is working.");
        }
    }
}
