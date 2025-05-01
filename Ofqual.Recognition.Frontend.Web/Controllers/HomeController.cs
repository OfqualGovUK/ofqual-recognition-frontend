using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        try
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                return NotFound();
            }
                       

        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading home: {Message}\r\nStackTrace: {StackTrace}",
                ex.Message, ex.StackTrace);
        }
        return View();
    }
}

