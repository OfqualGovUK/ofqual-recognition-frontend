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

        [Theory]
        [InlineData("Development", typeof(ViewResult))]
        [InlineData("PreProduction", typeof(ViewResult))]
        [InlineData("Production", typeof(NotFoundResult))]
        public void IndexPage_Returns_CorrectResult_BasedOnEnvironment(string environment, Type expectedType)
        {
            // Arrange
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);

            // Act
            var result = _sut.Index();

            // Assert
            Assert.IsType(expectedType, result);
        }
    }
}