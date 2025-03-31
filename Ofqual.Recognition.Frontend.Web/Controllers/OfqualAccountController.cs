using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Ofqual.Recognition.Frontend.Web.Controllers;


/// <summary>
/// Controller used in web apps to manage accounts.
/// Derived from https://github.com/AzureAD/microsoft-identity-web/blob/master/src/Microsoft.Identity.Web.UI/Areas/MicrosoftIdentity/Controllers/OfqualAccountController.cs
/// </summary>
[AllowAnonymous]
[Area("MicrosoftIdentity")]
[Route("[area]/[controller]/[action]")]
public class OfqualAccountController : Controller
{
    private readonly IOptionsMonitor<MicrosoftIdentityOptions> _optionsMonitor;

    /// <summary>
    /// Constructor of <see cref="OfqualAccountController"/> from <see cref="MicrosoftIdentityOptions"/>
    /// This constructor is used by dependency injection.
    /// </summary>
    /// <param name="microsoftIdentityOptionsMonitor">Configuration options.</param>
    public OfqualAccountController(IOptionsMonitor<MicrosoftIdentityOptions> microsoftIdentityOptionsMonitor)
    {
        _optionsMonitor = microsoftIdentityOptionsMonitor;
    }

    [HttpGet("{scheme?}")]
    public IActionResult SignIn([FromRoute] string scheme)
    {
        scheme ??= OpenIdConnectDefaults.AuthenticationScheme;
        var redirectUrl = Url.Content("~/");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        properties.Items["policy"] = Environment.GetEnvironmentVariable("AzureAdB2C__SignUpSignInPolicyId");
        return Challenge(properties, scheme);
    }

    /// <summary>
    /// Handles the user sign-out.
    /// </summary>
    /// <param name="scheme">Authentication scheme.</param>
    /// <returns>Sign out result.</returns>
    [HttpGet("{scheme?}")]
    public async Task<IActionResult> SignOutAsync(
        [FromRoute] string scheme)
    {
        scheme ??= OpenIdConnectDefaults.AuthenticationScheme;

        // obtain the id_token
        var idToken = await HttpContext.GetTokenAsync("id_token");
        // send the id_token value to the authentication middleware
        var redirectUrl = Url.Content("~/");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        properties.Items["id_token_hint"] = idToken;

        return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, scheme);
    }
}

