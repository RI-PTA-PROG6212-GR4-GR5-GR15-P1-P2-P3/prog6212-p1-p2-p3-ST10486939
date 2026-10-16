
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using RaceDayPOE2.API.Controllers;
using RaceDayPOE2.API.Data;
using RaceDayPOE2.API.DTOs;
using RaceDayPOE2.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace RaceDayPOE2.API.Test
{
    public class EventControllerTests : IDisposable
    {
        private readonly RaceDayDbContext _context;

        public EventControllerTests()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new RaceDayDbContext(options);

            // Seed organizers used by the tests.
            _context.EventOrganizers.AddRange(
                new EventOrganizer
                {
                    organizerID = 10,
                    name = "Organizer One",
                    roleID = 2,
                    emailAddress = "organizer1@test.com"
                },
                new EventOrganizer
                {
                    organizerID = 20,
                    name = "Organizer Two",
                    roleID = 2,
                    emailAddress = "organizer2@test.com"
                }
            );

            // Seed a valid category.
            _context.Categories.Add(new Category
            {
                categoryID = 1,
                description = "Running"
            });

            // Seed an event belonging to Organizer One.
            _context.Events.Add(new Event
            {
                eventID = 300,
                organizerID = 10,
                eventName = "Original Race",
                eventLocation = "Pretoria",
                description = "Original event description",
                distance = "5",
                categoryID = 1,
                type = "Running",
                eventDate = DateTime.UtcNow.AddDays(30)
            });

            _context.SaveChanges();
        }

        // Creates a controller with the specified organizer ID claim.
        private EventController CreateController(int organizerID)
        {
            var controller = new EventController(_context);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    organizerID.ToString()
                ),
                new Claim(ClaimTypes.Role, "Organizer")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            return controller;
        }

        private static EventRequest ValidEventRequest()
        {
            return new EventRequest
            {
                EventName = "New Race",
                EventLocation = "Johannesburg",
                Description = "A new running event",
                Distance = "10",
                CategoryID = 1,
                Type = "Running",
                EventDate = DateTime.UtcNow.AddDays(60)
            };
        }

        private static void AssertStatusCode(
            IActionResult result,
            int expectedStatusCode)
        {
            var statusResult =
                Assert.IsAssignableFrom<IStatusCodeActionResult>(result);

            Assert.Equal(expectedStatusCode, statusResult.StatusCode);
        }

        // 1. Creating a valid event succeeds.
        [Fact]
        public async Task CreateEvent_ValidRequest_CreatesEvent()
        {
            var controller = CreateController(10);
            var request = ValidEventRequest();

            var result = await controller.CreateEvent(request);

            AssertStatusCode(result, StatusCodes.Status201Created);

            var createdEvent = Assert.Single(
                _context.Events.Where(e => e.eventName == "New Race")
            );

            Assert.Equal(10, createdEvent.organizerID);
            Assert.Equal(1, createdEvent.categoryID);
            Assert.Equal("Johannesburg", createdEvent.eventLocation);
        }

        // 2. Creating an event with a category that does not exist fails.
        [Fact]
        public async Task CreateEvent_InvalidCategory_ReturnsBadRequest()
        {
            var controller = CreateController(10);
            var request = ValidEventRequest();

            request.CategoryID = 999;

            var result = await controller.CreateEvent(request);

            AssertStatusCode(result, StatusCodes.Status400BadRequest);

            Assert.DoesNotContain(
                _context.Events,
                e => e.eventName == "New Race"
            );
        }

        // 3. An event with no valid date is rejected.
        [Fact]
        public async Task CreateEvent_InvalidDate_ReturnsBadRequest()
        {
            var controller = CreateController(10);
            var request = ValidEventRequest();

            request.EventDate = default;

            var result = await controller.CreateEvent(request);

            AssertStatusCode(result, StatusCodes.Status400BadRequest);
        }

        // 4. An organizer who does not exist cannot create an event.
        [Fact]
        public async Task CreateEvent_UnknownOrganizer_ReturnsUnauthorized()
        {
            // Organizer 999 is deliberately not seeded in the context.
            var controller = CreateController(999);
            var request = ValidEventRequest();

            var result = await controller.CreateEvent(request);

            AssertStatusCode(result, StatusCodes.Status401Unauthorized);

            Assert.DoesNotContain(
                _context.Events,
                e => e.eventName == "New Race"
            );
        }

        // 5. An organizer can update their own event.
        [Fact]
        public async Task UpdateEvent_OwnEvent_UpdatesEvent()
        {
            var controller = CreateController(10);
            var request = ValidEventRequest();

            request.EventName = "Updated Race";

            var result = await controller.UpdateEvent(300, request);

            AssertStatusCode(result, StatusCodes.Status200OK);

            var updatedEvent = await _context.Events.FindAsync(300);

            Assert.NotNull(updatedEvent);
            Assert.Equal("Updated Race", updatedEvent.eventName);
            Assert.Equal(10, updatedEvent.organizerID);
        }

        // 6. An organizer cannot update another organizer's event.
        [Fact]
        public async Task UpdateEvent_OtherOrganizersEvent_ReturnsForbid()
        {
            var controller = CreateController(20);
            var request = ValidEventRequest();

            request.EventName = "Unauthorized Update";

            var result = await controller.UpdateEvent(300, request);

            Assert.IsType<ForbidResult>(result);

            var unchangedEvent = await _context.Events.FindAsync(300);

            Assert.NotNull(unchangedEvent);
            Assert.Equal("Original Race", unchangedEvent.eventName);
            Assert.Equal(10, unchangedEvent.organizerID);
        }

        // 7. Updating an event that does not exist returns 404.
        [Fact]
        public async Task UpdateEvent_MissingEvent_ReturnsNotFound()
        {
            var controller = CreateController(10);
            var request = ValidEventRequest();

            var result = await controller.UpdateEvent(999, request);

            AssertStatusCode(result, StatusCodes.Status404NotFound);
        }

        // 8. An organizer can delete their own event.
        [Fact]
        public async Task DeleteEvent_OwnEvent_DeletesEvent()
        {
            var controller = CreateController(10);

            var result = await controller.DeleteEvent(300);

            AssertStatusCode(result, StatusCodes.Status200OK);

            var deletedEvent = await _context.Events.FindAsync(300);

            Assert.Null(deletedEvent);
        }

        // 9. An organizer cannot delete another organizer's event.
        [Fact]
        public async Task DeleteEvent_OtherOrganizersEvent_ReturnsForbid()
        {
            var controller = CreateController(20);

            var result = await controller.DeleteEvent(300);

            Assert.IsType<ForbidResult>(result);

            var retainedEvent = await _context.Events.FindAsync(300);

            Assert.NotNull(retainedEvent);
            Assert.Equal(10, retainedEvent.organizerID);
        }

        // 10. Deleting an event that does not exist returns 404.
        [Fact]
        public async Task DeleteEvent_MissingEvent_ReturnsNotFound()
        {
            var controller = CreateController(10);

            var result = await controller.DeleteEvent(999);

            AssertStatusCode(result, StatusCodes.Status404NotFound);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

