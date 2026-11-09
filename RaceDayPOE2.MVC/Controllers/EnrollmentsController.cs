using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RaceDayPOE2.MVC.Models.ViewModels;
using RaceDayPOE2.MVC.Services;
using System.Net;
using System.Text.Json;

namespace RaceDayPOE2.MVC.Controllers
{
    public class EnrollmentsController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public EnrollmentsController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        //Display logged in participant's enrollment
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Index", "Home");

            try
            {
                var enrollments = await _apiService.GetAsync<List<MyEnrollmentsViewModel>>(
                    "api/enrollments/me");

                return View(enrollments ?? new List<MyEnrollmentsViewModel>());
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                TempData["LoginMessage"] = "Please log in again to view your enrolments.";
                return RedirectToAction("Login", "Account");
            }
            catch (HttpRequestException)
            {
                ViewBag.ErrorMessage = "Unable to retrieve your enrolments. Please try again.";
                return View(new List<MyEnrollmentsViewModel>());
            }
        }

        //Enroll the participant in an event
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(EnrollmentViewModel model)
        {
            Console.WriteLine("ENROLL ACTION REACHED");
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
            {
                TempData["EnrollmentError"] = "Invalid enrolment information.";
                return RedirectToAction("Index", "Events");
            }

            try
            {
                await _apiService.PostAsync<EnrollmentViewModel, JsonElement>(
                    "api/enrollments", model);

                TempData["EnrollmentSuccess"] = "You have successfully enrolled in the event!";
                return RedirectToAction("Index", "Enrollments");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                TempData["EnrollmentError"] = "You are already enrolled in this event.";
                return RedirectToAction("Index", "Events");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Clear();
                TempData["LoginMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }
            catch (HttpRequestException ex)
            {
                TempData["EnrollmentError"] = $"API Status Code: {(int?)ex.StatusCode} | Error: {ex.Message}";

                return RedirectToAction("Index", "Events");
            }
        }
    }
}