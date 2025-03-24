using Moq;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class EligibilityControllerTests
{
    private EligibilityController _sut;
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly Mock<IEligibilityService> _eligibilityServiceMock;

    public EligibilityControllerTests()
    {
        _sessionServiceMock = new Mock<ISessionService>();
        _eligibilityServiceMock = new Mock<IEligibilityService>();

        _sut = new EligibilityController(_eligibilityServiceMock.Object, _sessionServiceMock.Object);
    }
}