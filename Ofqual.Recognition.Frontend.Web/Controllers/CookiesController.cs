using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("cookies")]
public class CookiesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}