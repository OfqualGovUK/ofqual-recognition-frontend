using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Ofqual.Recognition.Frontend.Core.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[AllowAnonymous]
[Area("MicrosoftIdentity")]
[Controller]
[Route("[Area]/[Controller]/[Action]")]
public class OfqualAccountController : Controller
{
    private readonly IOptionsMonitor<MicrosoftIdentityOptions> _optionsMonitor;
    private readonly ISessionService _sessionService;

    public OfqualAccountController(IOptionsMonitor<MicrosoftIdentityOptions> optionsMonitor, ISessionService sessionService)
    {
        _optionsMonitor = optionsMonitor;
        _sessionService = sessionService;
    }

    [HttpGet("{scheme?}")]
    public IActionResult SignIn([FromRoute] string scheme)
    {
        scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Content(RouteConstants.ApplicationConstants.APPLICATION_PATH)
        };
        properties.Items["policy"] = _optionsMonitor.CurrentValue.SignUpSignInPolicyId;

        return Challenge(properties, scheme);
    }

    [HttpGet("{scheme?}")]
    public async Task<IActionResult> SignOutAsync([FromRoute] string scheme)
    {
        scheme ??= OpenIdConnectDefaults.AuthenticationScheme;

        _sessionService.ClearAllSession();

        // obtain the id_token
        var idToken = await HttpContext.GetTokenAsync("id_token");
        // send the id_token value to the authentication middleware
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Content(RouteConstants.AuthConstants.SIGNED_OUT_PATH)
        };

        properties.Items[AuthConstants.TokenHintIdentifier] = idToken;

        return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, scheme);
    }
}
