using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.DTOs;
using RaceDayPOE2.API.Models;

namespace RaceDayPOE2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<Participant> _participantHasher = new();
        private readonly PasswordHasher<EventOrganizer> _organizerHasher = new();

        public AuthController(
            RaceDayDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.EmailAddress) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Role))
            {
                return BadRequest(
                    new { message = "Name, email, password and role are required." });
            }

            string email = request.EmailAddress.Trim().ToLowerInvariant();
            string role = request.Role.Trim();

            if(request.Password.Length < 8)
            {
                return BadRequest(
                    new { message = "Password must be at least 8 charcters long." });
            }

            if(role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
            {
                if(request.Age is null or <1 or >120)
                {
                    return BadRequest(
                        new { message = "Participants must provide a valid age." });
                }
            }
            else if(!role.Equals("Organizer", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    new { message = "Role must be Participant or Organizer." });
            }

            bool participantExists = await _context.Participants
                .AnyAsync(p => p.emailAddress.ToLower() == email);

            bool organizerExists = await _context.EventOrganizers
                .AnyAsync(o => o.emailAddress.ToLower() == email);

            if(participantExists || organizerExists)
            {
                return Conflict(
                    new { message = "An account with this email already exists." });
            }

            string roleName = role.Equals("Participant",
                StringComparison.OrdinalIgnoreCase)
                ? "Participant"
                : "Organizer";

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(r =>
                r.description.ToLower() == roleName.ToLower());

            if(userRole == null)
            {
                return StatusCode(500, new
                {
                    message = $"The {roleName} role was not found in UserRole."
                });
            }

            if (roleName == "Participant")
            {
                var participant = new Participant
                {
                    name = request.Name.Trim(),
                    age = request.Age!.Value,
                    roleID = userRole.roleID,
                    emailAddress = email
                };

                participant.password =
                    _participantHasher.HashPassword(
                        participant, request.Password);

                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();

                return StatusCode(201, new
                {
                    message = "Participant registered successfully.",
                    participantID = participant.participantID,
                    name = participant.name,
                    emailAddress = participant.emailAddress,
                    role = roleName
                });
            }
            else
            {
                var organizer = new EventOrganizer
                {
                    name = request.Name.Trim(),
                    roleID = userRole.roleID,
                    emailAddress = email
                };

                organizer.password = _organizerHasher.HashPassword(
                    organizer, request.Password);

                _context.EventOrganizers.Add(organizer);
                await _context.SaveChangesAsync();

                return StatusCode(201, new
                {
                    message = "Organizer registered successfully.",
                    organizerID = organizer.organizerID,
                    name = organizer.name,
                    emailAddress = organizer.emailAddress,
                    role = roleName
                });
            }
        }

        //POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmailAddress) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(
                    new { message = "Email address and password are required." });
            }

            string email = request.EmailAddress.Trim().ToLowerInvariant();

            var participant = await _context.Participants.FirstOrDefaultAsync(p =>
                p.emailAddress.ToLower() == email);

            if (participant != null && !string.IsNullOrEmpty(participant.password))
            {
                var result = _participantHasher.VerifyHashedPassword(
                    participant,
                    participant.password,
                    request.Password);

                if(result != PasswordVerificationResult.Failed)
                {
                    var role = await _context.UserRoles.FirstOrDefaultAsync(r =>
                    r.roleID == participant.roleID);

                    if (role == null)
                        return Unauthorized(new { message = "Invalid login details." });

                    return CreateLoginResponse(
                        participant.participantID,
                        participant.name,
                        participant.emailAddress,
                        role.description);
                }
            }

            var organizer = await _context.EventOrganizers.FirstOrDefaultAsync(o => 
            o.emailAddress.ToLower() == email);

            if(organizer != null && !string.IsNullOrEmpty(organizer.password))
            {
                var result = _organizerHasher.VerifyHashedPassword(
                    organizer,
                    organizer.password,
                    request.Password);

                if(result != PasswordVerificationResult.Failed)
                {
                    var role = await _context.UserRoles.FirstOrDefaultAsync(r =>
                    r.roleID == organizer.roleID);

                    if (role == null)
                        return Unauthorized(new { message = "Invalid login details." });

                    return CreateLoginResponse(
                        organizer.organizerID,
                        organizer.name,
                        organizer.emailAddress,
                        role.description);
                }
            }

            return Unauthorized(
                new { message = "Invalid email address or password." });
        }

        private IActionResult CreateLoginResponse(
            int userID,
            string name,
            string email,
            string role)
        {
            string? key = _configuration["Jwt:Key"];
            string? issuer = _configuration["Jwt:Issuer"];
            string? audience = _configuration["Jwt:Audience"];

            if(string.IsNullOrWhiteSpace(key) ||
                Encoding.UTF8.GetByteCount(key) < 32 ||
                string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience))
            {
                return StatusCode(500, new
                {
                    message = "JWT configuration is missing or invalid."
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userID.ToString()),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer:  issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return Ok(new
            {
                message = "Login successful.",
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = new
                {
                    id = userID,
                    name,
                    emailAddress = email,
                    role
                }
            }); 
        }
    }
}
