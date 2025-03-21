using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return NotFound();
            }
            
            return View();
        }
    }
}
