using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.DTOs;
using RaceDayPOE2.API.Models;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    //authorisation will be added for each method individually
    public class EventController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventController(RaceDayDbContext context)
        {
            _context = context;
        }

        //CREATING EVENTS - only organizers can create events
        //POST: /api/events
        [HttpPost]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> CreateEvent(
            [FromBody] EventRequest request)
        {
            if (!TryGetOrganizerId(out int organizerID))
            {
                return Unauthorized();
            }

            if (request.EventDate == default)
            {
                return BadRequest(new
                {
                    message = "A valid event date is required."
                });
            }

            bool organizerExists = await _context.EventOrganizers
                .AnyAsync(o => o.organizerID == organizerID);

            if (!organizerExists)
            {
                return Unauthorized(new
                {
                    message = "Organizer account not found."
                });
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.categoryID == request.CategoryID);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "The selected category does not exist."
                });
            }
            var newEvent = new RaceDayPOE2.API.Models.Event
            {
                organizerID = organizerID,
                eventName = request.EventName.Trim(),
                eventLocation = request.EventLocation.Trim(),
                description = request.Description.Trim(),
                distance = request.Distance.Trim(),
                categoryID = request.CategoryID,
                type = request.Type.Trim(),
                eventDate = request.EventDate
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Event created successfully.",
                newEvent.eventID
            });
        }

        //UPDATING EVENTS - only organizers of the event can update their own event
        //PUT: /api/events/{eventid}
        [HttpPut("{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> UpdateEvent(
            int id,
            [FromBody] EventRequest request)
        {
            if (!TryGetOrganizerId(out int organizerId))
            {
                return Unauthorized();
            }

            if (request.EventDate == default)
            {
                return BadRequest(new
                {
                    message = "A valid event date is required."
                });
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.eventID == id);

            if (existingEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (existingEvent.organizerID != organizerId)
            {
                return Forbid();
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.categoryID == request.CategoryID);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "The selected category does not exist."
                });
            }

            existingEvent.eventName = request.EventName.Trim();
            existingEvent.eventLocation = request.EventLocation.Trim();
            existingEvent.description = request.Description.Trim();
            existingEvent.distance = request.Distance.Trim();
            existingEvent.categoryID = request.CategoryID;
            existingEvent.type = request.Type.Trim();
            existingEvent.eventDate = request.EventDate;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                existingEvent.eventID,
                existingEvent.organizerID,
                existingEvent.eventName,
                existingEvent.eventLocation,
                existingEvent.description,
                existingEvent.distance,
                existingEvent.categoryID,
                existingEvent.type,
                existingEvent.eventDate
            });
        }

        //DELETE EVENTS - deletes or cancels an event managed by organizer
        //DELETE: /api/events/{eventId}
        [HttpDelete("{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (!TryGetOrganizerId(out int organizerId))
            {
                return Unauthorized();
            }

            var existingEvent = await _context.Events.FirstOrDefaultAsync(e => e.eventID == id);

            if (existingEvent == null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            if (existingEvent.organizerID != organizerId)
            {
                return Forbid();
            }

            _context.Events.Remove(existingEvent);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "The event cannot be deleted because related records may exist."
                });
            }

            return Ok(new
            {
                message = "Event deleted successfully."
            });
        }

        //VIEW EVENTS
        //GET: api/events
        //participants & organizers can view all the events
        [HttpGet]
        [Authorize(Roles = "Participant,Organizer")]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        //Gets the organizer ID from the logged-in user's JWT
        private bool TryGetOrganizerId(out int organizerId)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(idClaim, out organizerId);
        }
    }
}
