using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.DTOs;
using RaceDayPOE2.API.Models;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public EnrollmentController(RaceDayDbContext context)
        {
            _context = context;
        }

        //POST: api/enrollments
        //participant enrolls in event & select category
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enroll(
            [FromBody] EnrollmentRequest request)
        {
            string? participantIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(participantIdClaim, out int participantId))
            {
                return Unauthorized("Invalid participant ID.");
            }
            
            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.eventID == request.EventID);

            if(raceEvent == null)
            {
                return NotFound("Event not found.");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.categoryID == request.CategoryID);

            if(category == null)
            {
                return NotFound("Category not found");
            }

            //current Event model has one categoryID per event
            //therefore, selected category must match that event
            if(raceEvent.categoryID != request.CategoryID)
            {
                return BadRequest(
                    "The selected category does not belong to this event.");
            }

            //prevent same participant enrolling twice
            bool alreadyEnrolled = await _context.Entries
                .AnyAsync(e =>
                e.eventID == request.EventID &&
                e.participantID == participantId);

            if(alreadyEnrolled)
            {
                return Conflict(
                    "You are already enrolled in this event.");
            }

            var enrollment = new Entries
            {
                eventID = request.EventID,
                participantID = participantId,
                categoryID = request.CategoryID
            };

            _context.Entries.Add(enrollment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Enrollment successfull.",
                enrollment.entryID,
                enrollment.participantID,
                enrollment.eventID,
                enrollment.categoryID
            });
        }

        //GET api/enrollments/me
        //participants can view only theyr own enrollments 
        [HttpGet("me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            string? participantIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(participantIdClaim, out int participantId))
            {
                return Unauthorized("Invalid participant ID.");
            }

            var enrollments = await _context.Entries
                .AsNoTracking()
                .Where(e => e.participantID == participantId)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.entryID,
                    e.eventID,

                    eventName = e.Event != null
                        ? e.Event.eventName
                        : "Event name unavailable",

                    e.categoryID,

                    CategoryDescription = e.Category != null
                        ? e.Category.description 
                        : "Category unavailable"
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        //GET: api/enrollments/event/300
        //an organizer can view enrollments for their own event
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> GetEventEnrollments(int eventId)
        {
            string? organizerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(!int.TryParse(organizerIdClaim, out int organizerId))
            {
                return Unauthorized("Invalid organizer ID.");
            }

            var raceEvent = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.eventID == eventId);
            
            if(raceEvent == null)
            {
                return NotFound("Event not found.");
            }


            if (raceEvent.organizerID != organizerId)
            {
                return Forbid();
            }

            var enrollments = await _context.Entries
                .AsNoTracking()
                .Where(e => e.eventID == eventId)
                .Select(e => new
                {
                    e.entryID,
                    e.eventID,
                    e.participantID,
                    e.categoryID
                })
                .ToListAsync();
            return Ok(enrollments);

        }
    }
}
