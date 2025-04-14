using Ofqual.Recognition.Frontend.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class ErrorControllerTests
{
    private readonly ErrorController _controller;

    public ErrorControllerTests()
    {
        _controller = new ErrorController();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NotFoundPage_ReturnsNotFoundView()
    {
        // Act
        var result = _controller.NotFoundPage();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("NotFound", viewResult.ViewName);
    }
}