using Xunit;
using Moq;
using System.Threading.Tasks;
using Ofqual.Recognition.Frontend.Infrastructure.Services;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using Microsoft.Identity.Web;
using System;
using Ofqual.Recognition.Frontend.Core.Constants;
using Moq.Protected;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Services;
public class ApplicationServiceTests
{
    private readonly Mock<IRecognitionCitizenClient> _mockRecognitionCitizenClient;
    private readonly Mock<ISessionService> _mockSession;
    private readonly HttpClient _httpClient;
    private readonly Mock<HttpMessageHandler> _mockMessageHandler;
    private readonly ApplicationService _applicationService;

    public ApplicationServiceTests()
    {
        _mockRecognitionCitizenClient = new Mock<IRecognitionCitizenClient>();
        _mockSession = new Mock<ISessionService>();
        // Calls to the HttpClient are actually convenience methods that you cannot mock directly, so we use a mock HttpMessageHandler instead.
        _mockMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_mockMessageHandler.Object)
        {
            BaseAddress = new Uri("http://example.com")
        };
        _mockRecognitionCitizenClient.Setup(c => c.GetClientAsync(It.IsAny<bool>())).ReturnsAsync(_httpClient);
        _applicationService = new ApplicationService(_mockRecognitionCitizenClient.Object, _mockSession.Object);
    }

    [Fact]
    public async Task InitialiseApplication_ReturnsApplication_FromSession()
    {
        // Arrange
        var expectedApp = new Application { ApplicationId = Guid.NewGuid(), Submitted = false };

        _mockSession.Setup(s => s.HasInSession(SessionKeys.Application)).Returns(true);
        _mockSession.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(expectedApp);

        // Act
        var result = await _applicationService.InitialiseApplication();

        // Assert
        Assert.Equal(expectedApp, result);
        _mockSession.Verify(s => s.GetFromSession<Application>(SessionKeys.Application), Times.Once);
        _mockRecognitionCitizenClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InitialiseApplication_CreatesApplication_WhenNotInSession()
    {
        // Arrange
        var preEngagementAnswers = new List<PreEngagementAnswer>();
        Guid applicationId = Guid.NewGuid();
        var expectedApp = new Application { ApplicationId = applicationId, Submitted = false };

        _mockSession.Setup(s => s.HasInSession(SessionKeys.Application)).Returns(false);
        _mockSession.Setup(s => s.GetFromSession<List<PreEngagementAnswer>>(SessionKeys.PreEngagementAnswers)).Returns(preEngagementAnswers);

        // Calls to the HttpClient are actually convenience methods that you cannot mock directly, so we use a mock HttpMessageHandler instead.
        _mockMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedApp)
            })
            .Verifiable();

        // Act
        var result = await _applicationService.InitialiseApplication();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedApp.ApplicationId, result!.ApplicationId);
        _mockSession.Verify(s => s.SetInSession(SessionKeys.Application, It.IsAny<Application>()), Times.Once);
        _mockSession.Verify(s => s.ClearFromSession(SessionKeys.PreEngagementAnswers), Times.Once);
    }

    [Fact]
    public async Task InitialiseApplication_ReturnsNull_OnApiFailure()
    {
        // Arrange
        var preEngagementAnswers = new List<PreEngagementAnswer>();

        _mockSession.Setup(s => s.HasInSession(SessionKeys.Application)).Returns(false);
        _mockSession.Setup(s => s.GetFromSession<List<PreEngagementAnswer>>(SessionKeys.PreEngagementAnswers)).Returns(preEngagementAnswers);

        // Act
        var result = await _applicationService.InitialiseApplication();

        _mockMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
            })
            .Verifiable();

        // Assert
        Assert.Null(result);
        _mockSession.Verify(s => s.SetInSession(SessionKeys.Application, It.IsAny<Application>()), Times.Never);
        _mockSession.Verify(s => s.ClearFromSession(SessionKeys.PreEngagementAnswers), Times.Never);
    }

    [Fact]
    public async Task InitialiseApplication_Throws_OnAuthenticationException()
    {
        // Arrange  
        _mockSession.Setup(s => s.HasInSession(SessionKeys.Application)).Returns(false);
        _mockSession.Setup(s => s.GetFromSession<List<PreEngagementAnswer>>(SessionKeys.PreEngagementAnswers)).Returns(new List<PreEngagementAnswer>());

        var scopes = new[] { "scope1", "scope2" }; // Example scopes  
        var msalException = new Microsoft.Identity.Client.MsalUiRequiredException("errorCode", "errorMessage");
        var exception = new MicrosoftIdentityWebChallengeUserException(msalException, scopes, null);
        _mockRecognitionCitizenClient.Setup(c => c.GetClientAsync(It.IsAny<bool>())).ThrowsAsync(exception);

        // Act & Assert  
        await Assert.ThrowsAsync<MicrosoftIdentityWebChallengeUserException>(() => _applicationService.InitialiseApplication());
    }
}
