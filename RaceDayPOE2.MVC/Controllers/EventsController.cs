using Microsoft.AspNetCore.Mvc;
using RaceDayPOE2.MVC.Models.ViewModels;
using RaceDayPOE2.MVC.Services;
using System.Net;
using Microsoft.AspNetCore.Http;

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

        //GET CREATE - to display the event creation form & load categories
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organizer")
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new CreateEventViewModel();

            try
            {
                model.Categories = await _apiService.GetAsync<List<CategoryViewModel>>("api/categories")
                    ?? new List<CategoryViewModel>();

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["EventError"] =
                    "Unable to load event categories. Please try again.";

                return RedirectToAction("Index");
            }
        }

        //POST CREATE - sends organizer's event details to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organizer")
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model);
                return View(model);
            }

            try
            {
                var eventData = new
                {
                    eventName = model.EventName,
                    eventLocation = model.EventLocation,
                    description = model.Description,
                    distance = model.Distance,
                    categoryID = model.CategoryID,
                    type = model.Type,
                    eventDate = model.EventDate
                };

                await _apiService.PostAsync<object, System.Text.Json.JsonElement>("api/events", eventData);

                TempData["EventSuccess"] = "Event created successfully!";

                return RedirectToAction("Index");
            }
            catch(HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Unable to create event. API error: {ex.Message}");

                await LoadCategoriesAsync(model);
                return View(model);
            }
        }

        private async Task LoadCategoriesAsync(CreateEventViewModel model)
        {
            model.Categories = await _apiService.GetAsync<List<CategoryViewModel>>("api/categories")
                ?? new List<CategoryViewModel>();
        }
    }
}