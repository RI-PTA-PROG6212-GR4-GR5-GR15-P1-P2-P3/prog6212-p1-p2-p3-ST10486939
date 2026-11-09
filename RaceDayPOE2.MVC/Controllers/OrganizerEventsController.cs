using Microsoft.AspNetCore.Mvc;
using RaceDayPOE2.MVC.Models.ViewModels;
using RaceDayPOE2.MVC.Services;

namespace RaceDayPOE2.MVC.Controllers
{
    public class OrganizerEventsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public OrganizerEventsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // Only organizers may access this page.
            if (HttpContext.Session.GetString("UserRole") != "Organizer")
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var events = await _apiService
                    .GetAsync<List<EventViewModel>>("api/events/me");

                return View(events ?? new List<EventViewModel>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Unable to retrieve your events.";
                return View(new List<EventViewModel>());
            }
        }
    }
}
