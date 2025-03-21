using Microsoft.AspNetCore.Http;
using Moq;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Services;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Services;

// Implementation of ISession for testing
public class FakeSession : ISession
{
    private readonly Dictionary<string, byte[]> _sessionStorage = [];

    public IEnumerable<string> Keys => _sessionStorage.Keys;
    public string Id => Guid.NewGuid().ToString();
    public bool IsAvailable => true;

    public void Clear() => _sessionStorage.Clear();
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _sessionStorage.Remove(key);
    public void Set(string key, byte[] value) => _sessionStorage[key] = value;
    public bool TryGetValue(string key, out byte[] value) 
    {
        if (_sessionStorage.TryGetValue(key, out var foundValue))
        {
            value = foundValue;
            return true;
        }

        value = [];
        return false;
    } 
}

public class EligibilityServiceTests
{
    // Helper to create an instance of EligibilityService with a fake session
    // private IEligibilityService GetEligibilityService(ISession session)
    // {
    //     var httpContext = new DefaultHttpContext();
    //     httpContext.Session = session;

    //     var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    //     httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

    //     return new EligibilityService(httpContextAccessorMock.Object);
    // }

    // [Fact]
    // public void SaveAnswers_SavesNonEmptyValues()
    // {
    //     // Arrange
    //     var fakeSession = new FakeSession();
    //     var service = GetEligibilityService(fakeSession);

    //     // Act
    //     service.SaveAnswers("Yes", "No", "No");

    //     // Assert
    //     Assert.Equal("Yes", fakeSession.GetString("QuestionOne"));
    //     Assert.Equal("No", fakeSession.GetString("QuestionTwo"));
    //     Assert.Equal("No", fakeSession.GetString("QuestionThree"));
    // }

    // [Fact]
    // public void GetAnswers_ReturnsEmptyForUnsetValues()
    // {
    //     // Arrange
    //     var fakeSession = new FakeSession();
    //     var service = GetEligibilityService(fakeSession);

    //     // Act
    //     var model = service.GetAnswers();

    //     // Assert
    //     Assert.Equal(string.Empty, model.QuestionOne);
    //     Assert.Equal(string.Empty, model.QuestionTwo);
    //     Assert.Equal(string.Empty, model.QuestionThree);
    // }
}