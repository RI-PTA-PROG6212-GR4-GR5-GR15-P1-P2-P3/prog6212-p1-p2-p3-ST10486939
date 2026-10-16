
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace RaceDayPOE2.API.Test
{
    public class RoleAuthorizationTests
    {
        private const string TestScheme = "Test";

        private static WebApplicationFactory<Program> CreateFactory()
        {
            var databaseName = Guid.NewGuid().ToString();

            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["Jwt:Key"] =
                                    "TestOnlySigningKey12345678901234567890",
                                ["Jwt:Issuer"] = "RaceDayTests",
                                ["Jwt:Audience"] = "RaceDayTests"
                            });
                    });

                    builder.ConfigureTestServices(services =>
                    {
                        // Replace SQL Server with an isolated in-memory store.
                        var descriptors = services
                            .Where(d =>
                                d.ServiceType ==
                                    typeof(RaceDayDbContext) ||
                                d.ServiceType ==
                                    typeof(DbContextOptions<RaceDayDbContext>))
                            .ToList();

                        foreach (var descriptor in descriptors)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<RaceDayDbContext>(options =>
                            options.UseInMemoryDatabase(databaseName));

                        // Use controlled test authentication instead of JWT.
                        services.AddAuthentication(options =>
                        {
                            options.DefaultAuthenticateScheme = TestScheme;
                            options.DefaultChallengeScheme = TestScheme;
                            options.DefaultForbidScheme = TestScheme;
                        }).AddScheme<
                            AuthenticationSchemeOptions,
                            TestAuthenticationHandler>(
                                TestScheme, _ => { });

                        // Seed data into the test database.
                        using var scope = services.BuildServiceProvider()
                            .CreateScope();

                        var db = scope.ServiceProvider
                            .GetRequiredService<RaceDayDbContext>();

                        db.EventOrganizers.Add(
                            new EventOrganizer
                            {
                                organizerID = 10,
                                name = "Test Organizer",
                                roleID = 2,
                                emailAddress = "organizer@test.com"
                            });

                        db.Participants.Add(new Participant
                        {
                            participantID = 100,
                            name = "Test Participant",
                            age = 25,
                            roleID = 1,
                            emailAddress = "particiant@test.com"
                        });

                        db.Categories.Add(new Category
                        {
                            categoryID = 1,
                            description = "Running"
                        });

                        db.Events.Add(new Event
                        {
                            eventID = 300,
                            organizerID = 10,
                            eventName = "Test Race",
                            eventLocation = "Pretoria",
                            description = "Test event",
                            distance = "5",
                            categoryID = 1,
                            type = "Running",
                            eventDate = DateTime.UtcNow.AddDays(30)
                        });

                        db.SaveChanges();
                    });
                });
        }

        private static HttpRequestMessage RequestAs(
            HttpMethod method,
            string url,
            string? role,
            int userId = 10)
        {
            var request = new HttpRequestMessage(method, url);

            if (role != null)
            {
                request.Headers.Add("X-Test-Role", role);
                request.Headers.Add("X-Test-UserId", userId.ToString());
            }

            return request;
        }

        private static StringContent ValidEventJson()
        {
            var eventRequest = new
            {
                EventName = "New Race",
                EventLocation = "Johannesburg",
                Description = "Test event description",
                Distance = "10 km",
                CategoryID = 1,
                Type = "Running",
                EventDate = DateTime.UtcNow.AddDays(60)
            };

            return new StringContent(
                System.Text.Json.JsonSerializer.Serialize(eventRequest),
                System.Text.Encoding.UTF8,
                "application/json");
        }

        [Fact]
        public async Task Participant_CannotCreateEvent_ReturnsForbidden()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var request = RequestAs(
                HttpMethod.Post, "/api/events", "Participant");

            request.Content = ValidEventJson();

            using var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotUpdateEvent_ReturnsForbidden()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var request = RequestAs(
                HttpMethod.Put, "/api/events/300", "Participant");

            request.Content = ValidEventJson();

            using var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CannotDeleteEvent_ReturnsForbidden()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            using var response = await client.SendAsync(
                RequestAs(
                    HttpMethod.Delete, "/api/events/300", "Participant"));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UnauthenticatedUser_CannotCreateEvent_ReturnsUnauthorized()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var request = RequestAs(
                HttpMethod.Post, "/api/events", null);

            request.Content = ValidEventJson();

            using var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Organizer_CanCreateEvent_ReturnsCreated()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var request = RequestAs(
                HttpMethod.Post, "/api/events", "Organizer");

            request.Content = ValidEventJson();

            using var response = await client.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == HttpStatusCode.Created,
                $"Expected 201 Created, but recieved {(int)response.StatusCode}" +
                $"{response.StatusCode}. Response: {responseBody}");
        }

        [Fact]
        public async Task Participant_CanViewEvents_ReturnsSuccess()
        {
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            using var response = await client.SendAsync(
                RequestAs(HttpMethod.Get, "/api/events", "Participant"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        //ENROLLMENT TESTS
        [Fact]
        public async Task Participant_CanEnrollInEvent_EnrollmentIsRecorded()
        {
            // Arrange
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/enrollments");

            request.Headers.Add("X-Test-Role", "Participant");
            request.Headers.Add("X-Test-UserId", "100");

            request.Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    EventID = 300,
                    CategoryID = 1
                }),
                Encoding.UTF8,
                "application/json");

            // Act
            using var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert that enrollment succeeded
            Assert.True(
                response.IsSuccessStatusCode,
                $"Expected a successful enrollment, but received " +
                $"{(int)response.StatusCode} {response.StatusCode}. " +
                $"Response: {responseBody}");

            // Verify the enrollment was saved in the test database
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<RaceDayDbContext>();

            var enrollment = await db.Entries
                .AsNoTracking()
                .SingleOrDefaultAsync(e =>
                    e.participantID == 100 &&
                    e.eventID == 300);

            Assert.NotNull(enrollment);
            Assert.Equal(100, enrollment.participantID);
            Assert.Equal(300, enrollment.eventID);
            Assert.Equal(1, enrollment.categoryID);
        }

        [Fact]
        public async Task Participant_CannotEnrollInSameEventTwice()
        {
            // Arrange
            using var factory = CreateFactory();
            using var client = factory.CreateClient();

            async Task<HttpResponseMessage> EnrollAsync()
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "/api/enrollments");

                request.Headers.Add("X-Test-Role", "Participant");
                request.Headers.Add("X-Test-UserId", "100");

                request.Content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        EventID = 300,
                        CategoryID = 1
                    }),
                    Encoding.UTF8,
                    "application/json");

                return await client.SendAsync(request);
            }

            // Act: first enrollment
            using var firstResponse = await EnrollAsync();

            Assert.True(
                firstResponse.IsSuccessStatusCode,
                $"First enrollment failed: {await firstResponse.Content.ReadAsStringAsync()}");

            // Act: attempt duplicate enrollment
            using var secondResponse = await EnrollAsync();

            // Assert: duplicate should be rejected
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

            // Verify only one enrollment exists
            using var scope = factory.Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<RaceDayDbContext>();

            var enrollmentCount = await db.Entries
                .AsNoTracking()
                .CountAsync(e =>
                    e.participantID == 100 &&
                    e.eventID == 300);

            Assert.Equal(1, enrollmentCount);
        }
    }

    // Supplies a test identity based on request headers.
    public class TestAuthenticationHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-Role", out var role))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var userId = Request.Headers["X-Test-UserId"].FirstOrDefault()
                         ?? "10";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "RaceDay Test User"),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
