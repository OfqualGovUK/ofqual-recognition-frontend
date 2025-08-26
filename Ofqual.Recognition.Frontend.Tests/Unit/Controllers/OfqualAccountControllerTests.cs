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
using Serilog;
using Microsoft.Extensions.Logging;
using ILogger = Serilog.ILogger;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class OfqualAccountControllerTests
{
    private readonly Mock<IOptionsMonitor<MicrosoftIdentityOptions>> _optionsMock = new();
    private readonly Mock<ISessionService> _sessionServiceMock = new();
    private readonly Mock<IUrlHelper> _urlHelperMock = new();
    private readonly OfqualAccountController _controller;

    public OfqualAccountControllerTests()
    {
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

    //[Theory]
    //[InlineData("AADB2C90091", "User canceled the operation. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90118", "User has forgotten their password. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90052", "Invalid username or password. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90054", "Invalid username or password. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90053", "Invalid username or password. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90225", "Invalid username or password. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90111", "Your account has been locked. Contact your support person to unlock it, then try again. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90114", "Your account is temporarily locked to prevent unauthorized use. Try again later. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90008", "The request does not contain a client ID parameter. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90006", "The redirect URI provided in the request is not registered for the client. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90007", "The application has no registered redirect URIs. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90035", "The service is temporarily unavailable. Please retry after a few minutes. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90036", "The request does not contain a URI to redirect the user to post logout. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90046", "We are having trouble signing you in. You might want to try starting your session over from the beginning. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90048", "An unhandled exception has occurred on the server. Error:{ErrorCode}. RequestId: {RequestId}")]
    //[InlineData("AADB2C90244", "There are too many requests at this moment. Please wait for some time and try again. Error:{ErrorCode}. RequestId: {RequestId}")]
    //public void GetB2CErrorMessage_ReturnsCorrectErrorMessage(string errorCode, string expectedMessage)
    //{
    //    // Arrange
    //    var sessionServiceMock = new Mock<ISessionService>();
    //    var optionsMonitorMock = new Mock<IOptionsMonitor<MicrosoftIdentityOptions>>();


    //    var loggerMock = new Mock<ILogger<OfqualAccountController>>();

    //    var controller = new OfqualAccountController(optionsMonitorMock.Object, sessionServiceMock.Object, loggerMock.Object);

    //    var httpContext = new DefaultHttpContext();
    //    httpContext.TraceIdentifier = "test-request-id";
    //    controller.ControllerContext = new ControllerContext
    //    {
    //        HttpContext = httpContext
    //    };

    //    // Act
    //    controller.Error(errorCode);

    //    // Assert
    //    loggerMock.Verify(
    //        x => x.Log(
    //            LogLevel.Error,
    //            It.IsAny<EventId>(),
    //            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error:{errorCode}. RequestId: test-request-id")),
    //            It.IsAny<Exception>(),
    //            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    //        Times.Once);
    //}

    [Theory]
    [InlineData("AADB2C90091")]
    [InlineData("AADB2C90244")]
    [InlineData("UNKNOWN_CODE")]
    public void Error_ReturnsProblemView(string errorCode)
    {
        // Arrange
        var optionsMock = new Mock<IOptionsMonitor<MicrosoftIdentityOptions>>();
        var sessionServiceMock = new Mock<ISessionService>();
        var controller = new OfqualAccountController(optionsMock.Object, sessionServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-request-id";
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = controller.Error(errorCode);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Problem", viewResult.ViewName);
    }
}