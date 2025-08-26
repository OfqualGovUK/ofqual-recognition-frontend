using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Ofqual.Recognition.Frontend.Core.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Serilog;

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

    [HttpGet]
    [Route("OfqualAccount/Error")]
    public IActionResult Error(string? encodedErrorCode = null)
    {
        var requestId = HttpContext.TraceIdentifier;

        if (!string.IsNullOrEmpty(encodedErrorCode))
        {
            switch (encodedErrorCode)
            {
                case "AADB2C90091":
                    Log.Error("User canceled the operation. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90118":
                    Log.Error("User has forgotten their password. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90052":
                case "AADB2C90054":
                case "AADB2C90053":
                case "AADB2C90225":
                    Log.Error("Invalid username or password. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90111":
                    Log.Error("Your account has been locked. Contact your support person to unlock it, then try again. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90114":
                    Log.Error("Your account is temporarily locked to prevent unauthorized use. Try again later. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90008":
                    Log.Error("The request does not contain a client ID parameter. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90006":
                    Log.Error("The redirect URI provided in the request is not registered for the client. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90007":
                    Log.Error("The application has no registered redirect URIs. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90035":
                    Log.Error("The service is temporarily unavailable. Please retry after a few minutes. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90036":
                    Log.Error("The request does not contain a URI to redirect the user to post logout. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90046":
                    Log.Error("We are having trouble signing you in. You might want to try starting your session over from the beginning. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90048":
                    Log.Error("An unhandled exception has occurred on the server. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                case "AADB2C90244":
                    Log.Error("There are too many requests at this moment. Please wait for some time and try again. Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
                default:
                    Log.Error("B2C Authentication Error:{EncodedErrorCode}. RequestId: {RequestId}", encodedErrorCode, requestId);
                    break;
            }
        }

        return View("Problem");
    }
}
