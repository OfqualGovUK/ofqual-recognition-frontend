using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Common.RegisterFrontend.Controllers
{
    [Route("cookies")]
    public class CookiesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}