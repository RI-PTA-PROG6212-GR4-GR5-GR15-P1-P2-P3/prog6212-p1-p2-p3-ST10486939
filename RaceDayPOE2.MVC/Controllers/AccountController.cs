using Microsoft.AspNetCore.Mvc;
using RaceDayPOE2.MVC.Models;
using RaceDayPOE2.MVC.Models.ViewModels;
using RaceDayPOE2.MVC.Services;
using System.Net;

namespace RaceDayPOE2.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public AccountController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        //DISPLAY LOGIN PAGE
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //SEND LOGIN DETAILS TO THE API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "The login form contains invalid information.");
                return View(model);
            }
            try
            {
                var response = await _apiService.PostAsync<LoginViewModel, LoginResponse>(
                    "api/auth/login", model);

                if(response == null)
                {
                    ModelState.AddModelError(string.Empty, "The API returned an empty response.");

                    return View(model);
                }

                if(string.IsNullOrWhiteSpace(response.Token) || response.User == null)
                {
                    ModelState.AddModelError(string.Empty,
                        "The API login response is missing the token or user information.");

                    return View(model);
                }

                HttpContext.Session.SetString("JWTToken", response.Token);
                HttpContext.Session.SetString("UserID", response.User.Id.ToString());
                HttpContext.Session.SetString("UserName", response.User.Name);
                HttpContext.Session.SetString("UserEmail", response.User.EmailAddress);
                HttpContext.Session.SetString("UserRole", response.User.Role);

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"API error: {ex.Message}");
                return View(model);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Login error: {ex.Message}");

                return View(model);
            }
        }

        //REGISTRATION
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var registrationData = new
                {
                    name = model.Name,
                    emailAddress = model.EmailAddress,
                    password = model.Password,
                    role = model.Role,
                    age = model.Age
                };

                await _apiService.PostAsync<object, System.Text.Json.JsonElement>(
                    "api/auth/register", registrationData);

                TempData["RegistrationSuccess"] =
                    "Your account has been created successfully. Please log in.";

                return RedirectToAction("Login");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty,
                    "Registration failed. Please check your details and try again.");

                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration.");

                return View(model);
            }
        }

        //LOG OUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
