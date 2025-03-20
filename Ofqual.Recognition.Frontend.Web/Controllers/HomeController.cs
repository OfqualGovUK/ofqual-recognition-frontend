using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    public class HomeController : Controller
    {

        public HomeController(){ }

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
