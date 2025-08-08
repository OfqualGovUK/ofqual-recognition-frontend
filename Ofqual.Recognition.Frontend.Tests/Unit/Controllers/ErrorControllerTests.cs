using Microsoft.AspNetCore.Mvc;
using Moq;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.Controllers;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class ErrorControllerTests
{
    private readonly ErrorController _controller;

    public ErrorControllerTests()
    {
        var mockHelpDeskContact = new Mock<HelpDeskContact>();
        _controller = new ErrorController(mockHelpDeskContact.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NotFoundPage_ReturnsNotFoundView()
    {
        // Act
        var result = _controller.NotFoundError();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("NotFound", viewResult.ViewName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InternalServerError_ReturnsProblemView()
    {
        // Act
        var result = _controller.InteralServerError();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Problem", viewResult.ViewName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void BadRequestError_ReturnsProblemView()
    {
        // Act
        var result = _controller.BadRequestError();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Problem", viewResult.ViewName);
    }
}