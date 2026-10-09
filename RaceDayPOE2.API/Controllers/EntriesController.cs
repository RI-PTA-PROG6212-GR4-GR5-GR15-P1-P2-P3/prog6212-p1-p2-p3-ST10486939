using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntriesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetEntries()
        {
            return Ok("Entries API is working.");
        }
    }
}
