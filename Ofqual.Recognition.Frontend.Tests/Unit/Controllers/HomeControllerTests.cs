using Ofqual.Recognition.Frontend.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class HomeControllerTests
{
    private HomeController _controller;

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
}