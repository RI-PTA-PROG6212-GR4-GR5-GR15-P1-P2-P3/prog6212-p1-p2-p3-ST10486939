using Microsoft.AspNetCore.Mvc;
using RaceDayPOE2.MVC.Models.ViewModels;
using RaceDayPOE2.MVC.Services;
using System.Net;

namespace RaceDayPOE2.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public EventsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _apiService.GetAsync<List<EventViewModel>>(
                    "api/events");

                return View(events ?? new List<EventViewModel>());
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();

                TempData["LoginMessage"] =
                    "Please log in to view events.";

                return RedirectToAction("Login", "Account");
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage =
                    "Unable to retrieve events. Please try again later.";

                return View(new List<EventViewModel>());
            }
        }
    }
}