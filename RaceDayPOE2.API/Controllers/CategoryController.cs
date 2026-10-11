using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.Models;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoryController(RaceDayDbContext context)
        {
            _context = context;
        }

        //GET: api/Category
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>>
            GetCategories()
        {
            var categories = await _context.Set<Category>()
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        //GET: api/Category/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Category>>
            GetCategory(int id)
        {
            var category = await _context.Set<Category>()
                .FindAsync(id);

            if(category == null)
            {
                return NotFound(
                    $"Category with ID {id} was not found.");
            }
            return Ok(category);
        }

        //POST: api/Category
        [HttpPost]
        public async Task<ActionResult<Category>>
            CreateCategory(Category category)
        {
            _context.Set<Category>().Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.categoryID },
                category);
        }

        //PUT: api/Category/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult>
            UpdateCategory(int id, Category category)
        {
            if (id != category.categoryID)
            {
                return BadRequest(
                    "The URL ID does not match the category ID.");
            }

            var existingCategory = await _context.Set<Category>().FindAsync(id);

            if (existingCategory == null)
            {
                return NotFound(
                    $"Category with ID {id} was not found.");
            }

            existingCategory.description = category.description;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //DELETE: api/Category/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult>
            DeleteCategory(int id)
        {
            var category = await _context.Set<Category>().FindAsync(id);

            if(category==null)
            {
                return NotFound(
                    $"Category with ID {id} was not found.");
            }
            _context.Set<Category>().Remove(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(
                    "This category may be linked to existing events.");
            }
            return NoContent();
        }
    }
}
