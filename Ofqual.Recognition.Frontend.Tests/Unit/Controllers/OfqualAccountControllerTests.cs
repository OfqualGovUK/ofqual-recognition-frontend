using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class OfqualAccountControllerTests
{
    private readonly Mock<IOptionsMonitor<MicrosoftIdentityOptions>> _optionsMock;
    private readonly Mock<IUrlHelper> _urlHelperMock;
    private readonly Mock<IAuthenticationService> _authenticationMock;
    private readonly Mock<IHttpContextAccessor> _contextMock;
    private readonly Mock<ISessionService> _sessionServiceMock;

    public OfqualAccountControllerTests()
    {
        _optionsMock = new Mock<IOptionsMonitor<MicrosoftIdentityOptions>>();
        _urlHelperMock = new Mock<IUrlHelper>();
        _authenticationMock = new Mock<IAuthenticationService>();
        _sessionServiceMock = new Mock<ISessionService>();
        _contextMock = new Mock<IHttpContextAccessor>();

        const string scheme = "test_scheme";

        _optionsMock
            .Setup(x => x.CurrentValue)
            .Returns(() => new MicrosoftIdentityOptions { SignUpSignInPolicyId = "B2C_Test_SUSI" });

        _urlHelperMock
            .Setup(x => x.Content(It.IsAny<string?>()))
            .Returns<string?>(_ => "http://localhost/home/index");

        _authenticationMock
            .Setup(x => x.AuthenticateAsync(_contextMock.Object.HttpContext!, scheme))
            .ReturnsAsync(() =>
            {
                var result = AuthenticateResult.Success(
                    new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(), scheme));

                result.Properties!.StoreTokens([ new AuthenticationToken
                {
                    Name = "id_token",
                    Value = Guid.NewGuid().ToString()
                }]);

                return result;
            });

        _contextMock
            .Setup(x => x.HttpContext!.RequestServices.GetService(typeof(IAuthenticationService)))
            .Returns(_authenticationMock.Object);
    }

    [Fact]
    public void SignIn_ReturnsChallenge()
    {
        //Arrange
        var controller = new OfqualAccountController(_optionsMock.Object, _sessionServiceMock.Object)
        {
            Url = _urlHelperMock.Object
        };

        //Act
        var result = controller.SignIn(It.IsAny<string>());

        //Assert
        Assert.IsType<ChallengeResult>(result);
    }

    [Fact]
    public async Task SignOut_ReturnsSignOut()
    {
        //Arrange
        var controller = new OfqualAccountController(_optionsMock.Object, _sessionServiceMock.Object)
        {
            Url = _urlHelperMock.Object,
            ControllerContext = new ControllerContext() 
            { 
                HttpContext = _contextMock.Object.HttpContext! 
            }
        };     

        //Act
        var result = await controller.SignOutAsync(It.IsAny<string>());

        //Assert
        Assert.IsType<SignOutResult>(result);
    }   
}