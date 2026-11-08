using Microsoft.AspNetCore.Mvc;

namespace RaceDayPOE2.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
