using Moq;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class ApplicationControllerTests
{
    private ApplicationController _controller;
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<IApplicationService> _applicationService;

    public ApplicationControllerTests()
    {
        _sessionServiceMock = new Mock<ISessionService>();
        _taskServiceMock = new Mock<ITaskService>();
        _applicationService = new Mock<IApplicationService>();

        _controller = new ApplicationController(_applicationService.Object, _taskServiceMock.Object, _sessionServiceMock.Object);
    }
}