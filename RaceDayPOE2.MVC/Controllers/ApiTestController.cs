using Microsoft.AspNetCore.Mvc;
using RaceDayPOE2.MVC.Services;
using System.Text.Json;

namespace RaceDayPOE2.MVC.Controllers
{
    public class ApiTestController : Controller
    {
        private readonly RaceDayApiService _apiService;

        public ApiTestController(RaceDayApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _apiService.GetAsync<JsonElement>(
                    "api/events");

                return Json(events);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    message = "Unable to communicate with the RaceDay API.",
                    details = ex.Message
                });
            }
        }
    }
}
