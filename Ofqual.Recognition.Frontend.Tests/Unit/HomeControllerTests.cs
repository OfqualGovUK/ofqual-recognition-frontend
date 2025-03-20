using Microsoft.Extensions.Logging;
using Ofqual.Recognition.Frontend.Controllers;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Tests.Unit
{
    public class HomeControllerTests
    {
        private HomeController _sut; 
        public HomeControllerTests()
        {
            _sut = new HomeController();
        }

        [Fact]
        public async Task IndexPageReturnsOk()
        {
            var result = await _sut.Index();
            Assert.IsType<ViewResult>(result);
        }
    }
}