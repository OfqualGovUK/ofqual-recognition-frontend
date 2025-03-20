using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ofqual.Recognition.Frontend.Web.Controllers;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class HomeControllerTests
{
    private HomeController _sut;
    public HomeControllerTests()
    {
        _sut = new HomeController(new Mock<ILogger<HomeController>>().Object);
    }

    [Fact]
    public async Task IndexPageReturnsOk()
    {
        var result = await _sut.Index();
        Assert.IsType<ViewResult>(result);
    }
}