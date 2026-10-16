using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDayPOE2.API.Controllers;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.DTOs;
using RaceDayPOE2.API.Models;
using Xunit;

namespace RaceDayPOE2.API.Test
{
    public class AuthControllerTests
    {
        private const string TestPassword = "Password123!";

        private static async Task<(RaceDayDbContext Context, 
            AuthController Controller)> CreateTestEnvironment()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);

            //Seed the roles required by AuthController
            context.UserRoles.AddRange(
                new UserRole
                {
                    roleID = 1,
                    description = "Participant"
                },
                new UserRole
                {
                    roleID = 2,
                    description = "Organizer"
                });

            await context.SaveChangesAsync();

            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = 
                    "RaceDay-Test-Key-At-Least-32-Characters-Long!",
                ["Jwt:Issuer"] = "RaceDay.Tests",
                ["Jwt:Audience"] = "RaceDay.Tests"
            };

            IConfiguration configuration =
                new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var controller = new AuthController(context, configuration);

            return (context, controller);
        }

        //REGISTRATION SUCCESS TEST
        [Fact]
        public async Task Register_ValidParticipant_ReturnsCreatedAndHashesPassword()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var request = new RegisterRequest
                {
                    Name = "Test Participant",
                    EmailAddress = "participant@example.com",
                    Password = TestPassword,
                    Role = "Participant",
                    Age = 25
                };

                var result = await controller.Register(request);

                var created = Assert.IsType<ObjectResult>(result);
                Assert.Equal(201, created.StatusCode);

                var participant = await context.Participants
                    .SingleAsync(p => p.emailAddress == "participant@example.com");

                Assert.Equal("Test Participant", participant.name);
                Assert.Equal(25, participant.age);

                Assert.False(string.IsNullOrWhiteSpace(
                    participant.passwordHash));

                Assert.NotEqual(
                    TestPassword,
                    participant.passwordHash);

                var hasher = new PasswordHasher<Participant>();

                Assert.Equal(
                    PasswordVerificationResult.Success,
                    hasher.VerifyHashedPassword(
                        participant,
                        participant.passwordHash!,
                        TestPassword));
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Register_ValidOrganizer_ReturnsCreatedAndHashesPassword()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var request = new RegisterRequest
                {
                    Name = "Test Organizer",
                    EmailAddress = "organizer@example.com",
                    Password = TestPassword,
                    Role = "Organizer"
                };

                var result = await controller.Register(request);

                var created = Assert.IsType<ObjectResult>(result);
                Assert.Equal(201, created.StatusCode);

                var organizer = await context.EventOrganizers
                    .SingleAsync(o => o.emailAddress == "organizer@example.com");

                Assert.Equal("Test Organizer", organizer.name);

                Assert.False(string.IsNullOrWhiteSpace(
                    organizer.passwordHash));

                Assert.NotEqual(
                    TestPassword,
                    organizer.passwordHash);

                var hasher = new PasswordHasher<EventOrganizer>();

                Assert.Equal(
                    PasswordVerificationResult.Success,
                    hasher.VerifyHashedPassword(
                        organizer,
                        organizer.passwordHash!,
                        TestPassword));
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        //REGISTRATION FAILURE TESTS
        [Fact]
        public async Task Register_ShortPassword_ReturnsBadRequest()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var request = new RegisterRequest
                {
                    Name = "Test Participant",
                    EmailAddress = "participant@example.com",
                    Password = "short",
                    Role = "Participant",
                    Age = 25
                };

                var result = await controller.Register(request);

                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Empty(await context.Participants.ToListAsync());
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Register_InvalidRole_ReturnsBadRequest()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var request = new RegisterRequest
                {
                    Name = "Test User",
                    EmailAddress = "user@example.com",
                    Password = TestPassword,
                    Role = "Administrator",
                    Age = 25
                };

                var result = await controller.Register(request);

                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Empty(await context.Participants.ToListAsync());
                Assert.Empty(await context.EventOrganizers.ToListAsync());
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                context.Participants.Add(new Participant
                {
                    name = "Existing Participant",
                    age = 25,
                    roleID = 1,
                    emailAddress = "participant@example.com",
                    passwordHash = "existing-test-hash"
                });

                await context.SaveChangesAsync();

                var request = new RegisterRequest
                {
                    Name = "another participant",
                    EmailAddress = "PARTICIPANT@example.com",
                    Password = TestPassword,
                    Role = "Participant",
                    Age = 30
                };

                var result = await controller.Register(request);

                Assert.IsType<ConflictObjectResult>(result);
                Assert.Single(await context.Participants.ToListAsync());
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Register_InvalidParticipantAge_ReturnsBadRequest()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var request = new RegisterRequest
                {
                    Name = "Test Participant",
                    EmailAddress = "participant@example.com",
                    Password = TestPassword,
                    Role = "Participant",
                    Age = 0
                };

                var result = await controller.Register(request);

                Assert.IsType<BadRequestObjectResult>(result);
                Assert.Empty(await context.Participants.ToListAsync());
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        //LOGIN SUCCESS TESTS
        [Fact]
        public async Task Login_ValidParticipant_ReturnsJwtToken()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var participant = new Participant
                {
                    name = "Test Participant",
                    age = 25,
                    roleID = 1,
                    emailAddress = "participant@example.com"
                };

                participant.passwordHash =
                    new PasswordHasher<Participant>()
                    .HashPassword(participant, TestPassword);

                context.Participants.Add(participant);
                await context.SaveChangesAsync();

                var result = await controller.Login(new LoginRequest
                {
                    EmailAddress = "participant@example.com",
                    Password = TestPassword
                });

                var response = Assert.IsType<OkObjectResult>(result);

                Assert.Equal(200, response.StatusCode);
                Assert.NotNull(response.Value);

                var token = GetPropertyValue(response.Value, "token");

                Assert.NotNull(token);
                Assert.False(string.IsNullOrWhiteSpace(token.ToString()));
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Login_ValidOrganizer_ReturnsJwtToken()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var organizer = new EventOrganizer
                {
                    name = "Test Participant",
                    roleID = 2,
                    emailAddress = "organizer@example.com"
                };

                organizer.passwordHash =
                    new PasswordHasher<EventOrganizer>()
                    .HashPassword(organizer, TestPassword);

                context.EventOrganizers.Add(organizer);
                await context.SaveChangesAsync();

                var result = await controller.Login(new LoginRequest
                {
                    EmailAddress = "organizer@example.com",
                    Password = TestPassword
                });

                var response = Assert.IsType<OkObjectResult>(result);

                Assert.Equal(200, response.StatusCode);

                var token = GetPropertyValue(response.Value!, "token");

                Assert.NotNull(token);
                Assert.False(string.IsNullOrWhiteSpace(token.ToString()));
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        //LOGIN FAILURE TESTS
        [Fact]
        public async Task Login_UnknownEmail_ReturnsUnauthorized()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var result = await controller.Login(new LoginRequest
                {
                    EmailAddress = "uknown@example.com",
                    Password = TestPassword
                });

                Assert.IsType<UnauthorizedObjectResult>(result);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Login_IncorrectPassword_ReturnsUnauthorized()
        {
            var(context, controller) = await CreateTestEnvironment();
            
            try
            {
                var participant = new Participant
                {
                    name = "Test Participant",
                    age = 25,
                    roleID = 1,
                    emailAddress = "participant@example.com"
                };

                participant.passwordHash =
                    new PasswordHasher<Participant>()
                    .HashPassword(participant, TestPassword);

                context.Participants.Add(participant);
                await context.SaveChangesAsync();

                var result = await controller.Login(new LoginRequest
                {
                    EmailAddress = "participant@example.com",
                    Password = "WrongPassword123"
                });

                Assert.IsType<UnauthorizedObjectResult>(result);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        [Fact]
        public async Task Login_Missing_Password_ReturnBadRequest()
        {
            var (context, controller) = await CreateTestEnvironment();

            try
            {
                var result = await controller.Login(new LoginRequest
                {
                    EmailAddress = "participant@example.com",
                    Password = ""
                });

                Assert.IsType<BadRequestObjectResult>(result);
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        private static object? GetPropertyValue(
            object instance,
            string propertyName)
        {
            return instance.GetType().GetProperty(propertyName, BindingFlags.Public |
                    BindingFlags.Instance)
                ?.GetValue(instance);
        }
        
    }
    
}