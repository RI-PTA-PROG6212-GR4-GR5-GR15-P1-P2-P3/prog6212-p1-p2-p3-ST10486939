using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.DTOs;
using RaceDayPOE2.API.Models;

namespace RaceDayPOE2.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(Roles = "Participant,Organizer")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        private readonly PasswordHasher<Participant> _participantHasher = new();
        private readonly PasswordHasher<EventOrganizer> _organizerHasher = new();

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        //GET: api/Profile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            
            if (!TryGetCurrentUser(out int userId, out string role))
                return Unauthorized();

            //getting participants profile
            if (role == "Participant")
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.participantID == userId);

                if (participant == null)
                    return NotFound(new { message = "Participant profile not found." });

                return Ok(new
                {
                    participant.participantID,
                    participant.name,
                    participant.age,
                    participant.emailAddress,
                    role = "Participant"
                });
            }
            //getting organizer profile
            if (role == "Organizer")
            {
                var organizer = await _context.EventOrganizers
                    .FirstOrDefaultAsync(o => o.organizerID == userId);

                if (organizer == null)
                    return NotFound(new { message = "Organizer profile not found." });

                return Ok(new
                {
                    organizer.organizerID,
                    organizer.name,
                    organizer.emailAddress,
                    role = "Organizer"
                });
            }
            return Forbid();
        }

        //PUT: api/Profile/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] ProfileUpdateRequest request)
        {
            if (!TryGetCurrentUser(out int userId, out string role))
                return Unauthorized();

            if (request.Name != null && string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Name cannot be empty." });

            if(request.EmailAddress != null &&
                (string.IsNullOrWhiteSpace(request.EmailAddress) ||
                !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                .IsValid(request.EmailAddress)))
            {
                return BadRequest(new { message = "Enter a valid email address." });
            }
            if(request.NewPassword != null &&
                (string.IsNullOrWhiteSpace(request.NewPassword) ||
                request.NewPassword.Length < 8))
            {
                return BadRequest(new
                {
                    message = "The new password must be at least 8 characters long"
                });
            }

            if(role == "Participant")
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.participantID == userId);

                if(participant == null)
                    return NotFound(new { message = "Participant profile not found." });

                if (request.Age.HasValue && request.Age.Value <= 0)
                    return BadRequest(new { message = "Age must be greater than zero." });

                if(request.EmailAddress != null)
                {
                    bool emailExists =
                        await _context.Participants.AnyAsync(p =>
                        p.emailAddress == request.EmailAddress &&
                        p.participantID != userId)
                    ||
                        await _context.EventOrganizers.AnyAsync(o =>
                        o.emailAddress == request.EmailAddress);

                    if (emailExists)
                        return Conflict(new { message = "That email address is already in use." });
                    participant.emailAddress = request.EmailAddress;
                }

                if (request.Name != null)
                    participant.name = request.Name.Trim();

                if (request.Age.HasValue)
                    participant.age = request.Age.Value;

                if(request.NewPassword != null)
                {
                    participant.passwordHash = _participantHasher.HashPassword(
                        participant, request.NewPassword);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Participant profile updated successfully.",
                    participant.participantID,
                    participant.name,
                    participant.age,
                    participant.emailAddress,
                    role = "Participant"
                });
            }

            if(role == "Organizer")
            {
                var organizer = await _context.EventOrganizers
                    .FirstOrDefaultAsync(o => o.organizerID == userId);

                if (organizer == null)
                    return NotFound(new { message = "Organizer profile not found." });

                if (request.Age.HasValue)
                    return BadRequest(new
                    {
                        message = "Age can only be updated for participant profiles."
                    });

                if(request.EmailAddress != null)
                {
                    bool emailExists =
                        await _context.EventOrganizers.AnyAsync(o =>
                        o.emailAddress == request.EmailAddress &&
                        o.organizerID != userId)
                        ||
                        await _context.Participants.AnyAsync(p =>
                        p.emailAddress == request.EmailAddress);

                    if (emailExists)
                        return Conflict(new { message = "That email address is already in use." });
                    organizer.emailAddress = request.EmailAddress;
                }
                if (request.Name != null)
                    organizer.name = request.Name.Trim();
                if (request.NewPassword != null)
                {
                    organizer.passwordHash = _organizerHasher.HashPassword(
                        organizer, request.NewPassword);
                }
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Organizer profile updated successfully.",
                    organizer.organizerID,
                    organizer.name,
                    organizer.emailAddress,
                    role = "Organizer"
                });
            }
            return Forbid();
        }
        private bool TryGetCurrentUser(out int userId, out string role)
        {
            userId = 0;

            role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            string? idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrWhiteSpace(idClaim))
                return false;

            if(!int.TryParse(idClaim, out userId))
                return false;

            return role == "Participant" || role == "Organizer";
        }
    }
}
