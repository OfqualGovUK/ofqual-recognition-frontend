using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Security.Claims;
using Moq;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class OfqualAccountControllerTests
{
    private readonly Mock<IOptionsMonitor<MicrosoftIdentityOptions>> _optionsMock;
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly Mock<IUrlHelper> _urlHelperMock;
    private readonly OfqualAccountController _controller;

    public OfqualAccountControllerTests()
    {
        _optionsMock = new Mock<IOptionsMonitor<MicrosoftIdentityOptions>>();
        _sessionServiceMock = new Mock<ISessionService>();
        _urlHelperMock = new Mock<IUrlHelper>();

        _controller = new OfqualAccountController(_optionsMock.Object, _sessionServiceMock.Object)
        {
            Url = _urlHelperMock.Object
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SignIn_ReturnsChallenge_WithCorrectSchemeAndRedirect()
    {
        // Arrange
        var scheme = "custom-scheme";
        var expectedRedirect = "/application";
        var expectedPolicy = "test-policy";

        _urlHelperMock.Setup(u => u.Content(It.IsAny<string>())).Returns(expectedRedirect);
        _optionsMock.Setup(o => o.CurrentValue).Returns(new MicrosoftIdentityOptions
        {
            SignUpSignInPolicyId = expectedPolicy
        });

        // Act
        var result = _controller.SignIn(scheme);

        // Assert
        var challengeResult = Assert.IsType<ChallengeResult>(result);
        Assert.Equal(scheme, challengeResult.AuthenticationSchemes.Single());

        var redirectUri = challengeResult.Properties?.RedirectUri;
        var policyItem = challengeResult.Properties?.Items["policy"];

        Assert.Equal(expectedRedirect, redirectUri);
        Assert.Equal(expectedPolicy, policyItem);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SignOutAsync_ReturnsSignOutResult_AndClearsSession()
    {
        // Arrange
        var sessionServiceMock = new Mock<ISessionService>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<MicrosoftIdentityOptions>>();
        var options = new MicrosoftIdentityOptions
        {
            SignUpSignInPolicyId = "B2C_1_SignIn"
        };
        optionsMonitorMock.Setup(o => o.CurrentValue).Returns(options);

        var authServiceMock = new Mock<IAuthenticationService>();

        authServiceMock.Setup(a => a.AuthenticateAsync(It.IsAny<HttpContext>(), null))
            .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                new Claim("id_token", "mock-id-token")
                }, "mockAuth")), "mockAuth")));

        var services = new ServiceCollection();
        services.AddSingleton(authServiceMock.Object);
        services.AddAuthentication("mockAuth").AddCookie("mockAuth");
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(u => u.Content(It.IsAny<string>())).Returns("/signed-out");

        var controller = new OfqualAccountController(optionsMonitorMock.Object, sessionServiceMock.Object)
        {
            Url = urlHelperMock.Object,
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        // Act
        var result = await controller.SignOutAsync(null!);

        // Assert
        var signOutResult = Assert.IsType<SignOutResult>(result);
        Assert.Contains(CookieAuthenticationDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);
        Assert.Contains(OpenIdConnectDefaults.AuthenticationScheme, signOutResult.AuthenticationSchemes);

        sessionServiceMock.Verify(s => s.ClearAllSession(), Times.Once);
    }
}