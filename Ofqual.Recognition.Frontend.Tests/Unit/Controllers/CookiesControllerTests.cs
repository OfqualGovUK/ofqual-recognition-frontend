using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Web.Controllers;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class CookiesControllerTests
{
    private readonly CookiesController _controller;

    public CookiesControllerTests()
    {
        _controller = new CookiesController();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Index_ReturnsDefaultView()
    {
        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
    }
}