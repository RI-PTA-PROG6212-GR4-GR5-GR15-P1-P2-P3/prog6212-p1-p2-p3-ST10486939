using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.DTOs;
using RaceDayPOE2.API.Models;


using Microsoft.Identity.Client;
using System.Drawing.Text;

namespace RaceDayPOE2.API.Controllers
{
    [ApiController]
    [Route("api/results")]
    public class ParticipantResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        //POST: api/results
        //organizer records a participant's result
        [HttpPost]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> CreateResult(
            [FromBody] ResultRequest request)
        {
            string? organizerIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(organizerIdClaim, out int organizerId))
            {
                return Unauthorized("Invalid organizer ID.");
            }

            //find enrollment & the event's organizer
            var enrollment = await (
                from entry in _context.Entries
                join raceEvent in _context.Events on entry.eventID equals raceEvent.eventID
                where entry.entryID == request.EntryID 
                select new
                {
                    entry.entryID,
                    entry.participantID,
                    OrganizerID = raceEvent.organizerID
                }
           ).FirstOrDefaultAsync();

            if (enrollment == null)
            {
                return NotFound("Enrollment not found");
            }

            if(enrollment.OrganizerID != organizerId)
            {
                return Forbid();
            }

            bool resultExists = await _context.ParticipantResults
                .AnyAsync(r => r.entryID == request.EntryID);

            if(resultExists)
            {
                return Conflict(
                    "A result has already been recorded for this enrollment.");
            }

            var result = new ParticipantResults
            {
                entryID = enrollment.entryID,
                participantID = enrollment.participantID,
                completionTime = request.completiontime,
                finishingPosition = request.FinishingPosition
            };

            _context.ParticipantResults.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Participant result recorded successfully.",
                result.resultID,
                result.entryID,
                result.participantID,
                result.completionTime,
                result.finishingPosition
            });
        }

        //PUT: api/results/1
        //organizer updates existing result for their own events
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> UpdateResult (
            int id,
            [FromBody] ResultRequest request)
        {
            string? organizerIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(organizerIdClaim, out int organizerId))
            {
                return Unauthorized("Invalid organizer ID.");
            }

            var result = await _context.ParticipantResults
                .FirstOrDefaultAsync(r => r.resultID == id);

            if(result == null)
            {
                return NotFound("Results not found.");
            }

            //do not allow an update to move result to another enrollment
            if(result.entryID != request.EntryID)
            {
                return BadRequest(
                    "The enrollment ID cannot be changed when updating a result.");
            }

            var eventOwner = await (
                from entry in _context.Entries 
                join raceEvent in _context.Events 
                    on entry.eventID equals raceEvent.eventID
                where entry.entryID == result.entryID
                select raceEvent.organizerID
                ).FirstOrDefaultAsync();

            var enrollmentExists = await _context.Entries
                .AnyAsync(e => e.entryID == result.entryID);

            if(!enrollmentExists)
            {
                return NotFound("Enrollment not found.");
            }

            if(eventOwner != organizerId)
            {
                return Forbid();
            }

            result.completionTime = request.completiontime;
            result.finishingPosition = request.FinishingPosition;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully.",
                result.resultID,
                result.entryID,
                result.participantID,
                result.completionTime,
                result.finishingPosition
            });
        }

        //GET: api/results/me
        //participant views their own results
        [HttpGet("me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResult()
        {
            string? participantIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(participantIdClaim, out int participantId))
            {
                return Unauthorized("Invalid participant ID.");
            }

            var results = await (
                from result in _context.ParticipantResults.AsNoTracking()
                join entry in _context.Entries
                    on result.entryID equals entry.entryID
                join raceEvent in _context.Events
                    on entry.eventID equals raceEvent.eventID
                where result.participantID == participantId
                orderby result.finishingPosition
                select new
                {
                    result.resultID,
                    result.entryID,
                    EventID = raceEvent.eventID,
                    EventName = raceEvent.eventName,
                    result.finishingPosition
                }
                ).ToListAsync();

            return Ok(results);
        }

        //GET: api/results/event/300
        //organizer views results for their own event
        [HttpGet("event/{eventId:int}")]
        [Authorize(Roles = "Organizer")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            string? organizerIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

            if(raceEvent.organizerID != organizerId)
            {
                return Forbid();
            }

            var results = await (
                from result in _context.ParticipantResults.AsNoTracking()
                join entry in _context.Entries
                    on result.entryID equals entry.entryID
                join participant in _context.Participants
                    on result.participantID equals participant.participantID
                where entry.eventID == eventId
                orderby result.finishingPosition
                select new
                {
                    result.resultID,
                    result.entryID,
                    result.participantID,
                    ParticipantName = participant.name,
                    EventName = raceEvent.eventName,
                    result.completionTime,
                    result.finishingPosition
                }
                ).ToListAsync();

            return Ok(results);
        }
    }
}
