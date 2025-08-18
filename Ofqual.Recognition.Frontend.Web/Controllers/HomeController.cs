using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IFeatureFlagService _featureFlagService;

    public HomeController(IConfiguration configuration, IFeatureFlagService featureFlagService)
    {
        _configuration = configuration;
        _featureFlagService = featureFlagService;
    }

    public IActionResult Index()
    {
        if (_featureFlagService.IsFeatureEnabled("HideDevPage") ||
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
        {
            var redirectUrl = _configuration["GovUk:StartPage"];
            return string.IsNullOrWhiteSpace(redirectUrl)
                ? NotFound()
                : Redirect(redirectUrl);          
        
        }
        return View();
    }

    [HttpGet("signed-out")]
    public IActionResult SignedOut()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return Redirect("~/MicrosoftIdentity/OfqualAccount/SignOut");
        }

        return View();
    }
}

