using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRoleController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetCategories()
        {
            return Ok("Category API is working.");
        }
    }
}
