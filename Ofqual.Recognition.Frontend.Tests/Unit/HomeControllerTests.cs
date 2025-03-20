using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Web.Controllers;

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