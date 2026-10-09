using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetEvents()
        {
            return Ok("Events API is working.");
        }
    }
}
