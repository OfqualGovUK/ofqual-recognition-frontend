using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    [Route("404")]
    public IActionResult NotFoundPage()
    {
        return View("NotFound");
    }
}
