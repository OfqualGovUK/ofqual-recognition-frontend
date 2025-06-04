using Ofqual.Recognition.Frontend.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class HomeControllerTests
{
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _controller = new HomeController();
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("Development", typeof(ViewResult))]
    [InlineData("PreProduction", typeof(ViewResult))]
    [InlineData("Production", typeof(NotFoundResult))]
    public void IndexPage_Returns_CorrectResult_BasedOnEnvironment(string environment, Type expectedType)
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);

        // Act
        var result = _controller.Index();
        
        // Assert
        Assert.IsType(expectedType, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SignedOut_ReturnsRedirect_WhenUserIsAuthenticated()
    {
        // Arrange
        var controller = new HomeController();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }, "mock"))
        };
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = controller.SignedOut();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("~/MicrosoftIdentity/OfqualAccount/SignOut", redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void SignedOut_ReturnsView_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var controller = new HomeController();
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = controller.SignedOut();

        // Assert
        Assert.IsType<ViewResult>(result);
    }
}