using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
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
