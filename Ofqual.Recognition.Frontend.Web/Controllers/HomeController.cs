using Microsoft.AspNetCore.Mvc;

namespace ofqual_recognition_frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Production")
            {
                return View();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
