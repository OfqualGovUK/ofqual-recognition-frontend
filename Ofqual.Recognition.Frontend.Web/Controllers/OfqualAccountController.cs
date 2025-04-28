using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Ofqual.Recognition.Frontend.Core.Constants;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    [AllowAnonymous]
    [Area("MicrosoftIdentity")]
    [Route("[Area]/[Controller]/[Action]")]
    public class OfqualAccountController : Controller
    {
        private readonly IOptionsMonitor<MicrosoftIdentityOptions> _optionsMonitor;
        public OfqualAccountController(IOptionsMonitor<MicrosoftIdentityOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;        
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

        [HttpGet("{scheme?}")]
        public async Task<IActionResult> SignOutAsync([FromRoute] string scheme)
        {
            scheme ??= OpenIdConnectDefaults.AuthenticationScheme;

            // obtain the id_token
            var idToken = await HttpContext.GetTokenAsync("id_token");
            // send the id_token value to the authentication middleware
            var redirectUrl = Url.Content("~/");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items[AuthConstants.TokenHintIdentifier] = idToken;

            return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, scheme);
        }
    }
}
