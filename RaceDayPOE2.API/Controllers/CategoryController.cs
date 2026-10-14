using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayPOE2.API.Data;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize(Roles = "Participant,Organizer")]
    public class CategoryController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoryController(RaceDayDbContext context)
        {
            _context = context;
        }

        //GET: api/categories - participants & organizers can view categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.categoryID)
                .ToListAsync();
            return Ok(categories);
        }
    }
}
