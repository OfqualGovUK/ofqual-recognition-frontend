using Ofqual.Recognition.Frontend.Infrastructure.Services;
using Ofqual.Recognition.Frontend.Tests.TestData;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using Moq;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Services;

public class SessionServiceTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly SessionService _sessionService;

    public SessionServiceTests()
    {
        _sessionMock = new Mock<ISession>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.Setup(c => c.Session).Returns(_sessionMock.Object);
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);

        _sessionService = new SessionService(_httpContextAccessorMock.Object);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [MemberData(nameof(SessionServiceTestCases.SetInSessionTestCases), MemberType = typeof(SessionServiceTestCases))]
    public void SetInSession_ShouldCallSet_WithCorrectSerializedJson(string key, SessionServiceTestCases.TestData data)
    {
        // Arrange
        var expectedJson = JsonConvert.SerializeObject(data);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedJson);

        _sessionMock.Setup(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))))
                    .Verifiable();

        // Act
        _sessionService.SetInSession(key, data);

        // Assert
        _sessionMock.Verify(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetFromSession_ShouldReturnNull_WhenSessionIsNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new SessionService(_httpContextAccessorMock.Object);

        // Act
        var result = service.GetFromSession<SessionServiceTestCases.TestData>("anyKey");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetFromSession_ShouldReturnNull_WhenKeyNotFound()
    {
        // Arrange
        var key = "missingKey";
        byte[]? outBytes = null;

        _sessionMock.Setup(s => s.TryGetValue(key, out outBytes)).Returns(false);

        // Act
        var result = _sessionService.GetFromSession<SessionServiceTestCases.TestData>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFromSession_ShouldReturnDeserializedObject_WhenKeyExists()
    {
        // Arrange
        var key = "validKey";
        var testData = new SessionServiceTestCases.TestData { Name = "Expected" };
        var jsonData = JsonConvert.SerializeObject(testData);
        var bytes = Encoding.UTF8.GetBytes(jsonData);

        byte[]? outBytes = bytes;
        _sessionMock.Setup(s => s.TryGetValue(key, out outBytes)).Returns(true);

        // Act
        var result = _sessionService.GetFromSession<SessionServiceTestCases.TestData>(key);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Expected", result!.Name);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("existingKey", true)]
    [InlineData("missingKey", false)]
    public void HasInSession_ShouldReturnExpectedResult(string key, bool exists)
    {
        // Arrange
        byte[] dummy = Encoding.UTF8.GetBytes("some value");

        _sessionMock.Setup(s => s.TryGetValue(key, out dummy!))
                    .Returns(exists);

        // Act
        var result = _sessionService.HasInSession(key);

        // Assert
        Assert.Equal(exists, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ClearFromSession_ShouldRemoveKeyFromSession()
    {
        // Arrange
        var key = "keyToRemove";

        _sessionMock.Setup(s => s.Remove(key))
            .Verifiable();

        // Act
        _sessionService.ClearFromSession(key);

        // Assert
        _sessionMock.Verify(s => s.Remove(key), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ClearAllSession_ShouldClearAllSessionData()
    {
        // Arrange
        _sessionMock.Setup(s => s.Clear())
            .Verifiable();

        // Act
        _sessionService.ClearAllSession();

        // Assert
        _sessionMock.Verify(s => s.Clear(), Times.Once);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(StatusType.CannotStartYet, StatusType.Completed)]
    [InlineData(StatusType.InProgress, StatusType.CannotStartYet)]
    public void UpdateTaskStatusInSession_ShouldUpdateStatus_WhenTaskExists(StatusType originalStatus, StatusType newStatus)
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var task = new TaskItemStatus
        {
            TaskId = taskId,
            TaskName = "Test Task",
            Status = originalStatus,
            FirstQuestionURL = "/application-details/contact-details"
        };

        var section = new TaskItemStatusSection
        {
            SectionId = Guid.NewGuid(),
            SectionName = "Section A",
            Tasks = new List<TaskItemStatus> { task }
        };

        var taskSections = new List<TaskItemStatusSection> { section };
        var serialized = JsonConvert.SerializeObject(taskSections);
        var sessionBytes = Encoding.UTF8.GetBytes(serialized);

        _sessionMock.Setup(s => s.TryGetValue(SessionKeys.ApplicationTaskList, out sessionBytes))
            .Returns(true);

        byte[]? updatedBytes = null;
        _sessionMock.Setup(s => s.Set(SessionKeys.ApplicationTaskList, It.IsAny<byte[]>()))
            .Callback<string, byte[]>((_, bytes) => updatedBytes = bytes);

        // Act
        _sessionService.UpdateTaskStatusInSession(taskId, newStatus);

        // Assert
        Assert.NotNull(updatedBytes);

        var updatedSections = JsonConvert.DeserializeObject<List<TaskItemStatusSection>>(Encoding.UTF8.GetString(updatedBytes!));
        var updatedTask = updatedSections!.SelectMany(s => s.Tasks).First(t => t.TaskId == taskId);
        Assert.Equal(newStatus, updatedTask.Status);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(StatusType.Completed)]
    [InlineData(StatusType.InProgress)]
    public void GetTaskStatusFromSession_ShouldReturnCorrectStatus_WhenTaskExists(StatusType expectedStatus)
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskItemStatus
        {
            TaskId = taskId,
            TaskName = "Test Task",
            Status = expectedStatus,
            FirstQuestionURL = "/application-details/contact-details"
        };

        var section = new TaskItemStatusSection
        {
            SectionId = Guid.NewGuid(),
            SectionName = "Section A",
            Tasks = new List<TaskItemStatus> { task }
        };

        var taskSections = new List<TaskItemStatusSection> { section };
        var serialized = JsonConvert.SerializeObject(taskSections);
        var sessionBytes = Encoding.UTF8.GetBytes(serialized);

        _sessionMock.Setup(s => s.TryGetValue(SessionKeys.ApplicationTaskList, out sessionBytes))
            .Returns(true);

        // Act
        var result = _sessionService.GetTaskStatusFromSession(taskId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedStatus, result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetTaskStatusFromSession_ShouldReturnNull_WhenSessionIsNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext)null!);

        // Act
        var result = _sessionService.GetTaskStatusFromSession(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetTaskStatusFromSession_ShouldReturnNull_WhenKeyIsMissing()
    {
        // Arrange
        byte[]? dummy = null;

        _sessionMock.Setup(s => s.TryGetValue(SessionKeys.ApplicationTaskList, out dummy))
            .Returns(false);

        // Act
        var result = _sessionService.GetTaskStatusFromSession(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetTaskStatusFromSession_ShouldReturnNull_WhenTaskIdNotFound()
    {
        // Arrange
        var unrelatedTask = new TaskItemStatus
        {
            TaskId = Guid.NewGuid(),
            TaskName = "Another Task",
            Status = StatusType.NotStarted,
            FirstQuestionURL = "/something"
        };

        var section = new TaskItemStatusSection
        {
            SectionId = Guid.NewGuid(),
            SectionName = "Section X",
            Tasks = new List<TaskItemStatus> { unrelatedTask }
        };

        var taskSections = new List<TaskItemStatusSection> { section };
        var serialized = JsonConvert.SerializeObject(taskSections);
        var sessionBytes = Encoding.UTF8.GetBytes(serialized);
        _sessionMock.Setup(s => s.TryGetValue(SessionKeys.ApplicationTaskList, out sessionBytes))
            .Returns(true);

        // Act
        var result = _sessionService.GetTaskStatusFromSession(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UpsertPreEngagementAnswer_ShouldAddNewAnswer_WhenNotExists()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var answerJson = "{\"value\":\"test\"}";

        byte[]? sessionBytes = null;
        _sessionMock.Setup(s => s.TryGetValue(SessionKeys.PreEngagementAnswers, out sessionBytes))
            .Returns(false);

        byte[]? storedBytes = null;
        _sessionMock.Setup(s => s.Set(SessionKeys.PreEngagementAnswers, It.IsAny<byte[]>()))
            .Callback<string, byte[]>((_, bytes) => storedBytes = bytes);

        // Act
        _sessionService.UpsertPreEngagementAnswer(questionId, taskId, answerJson);

        // Assert
        Assert.NotNull(storedBytes);
        var stored = JsonConvert.DeserializeObject<List<PreEngagementAnswer>>(Encoding.UTF8.GetString(storedBytes!));
        Assert.Single(stored!);
        Assert.Equal(questionId, stored![0].QuestionId);
        Assert.Equal(taskId, stored[0].TaskId);
        Assert.Equal(answerJson, stored[0].AnswerJson);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [Trait("Category", "Unit")]
    public void UpsertPreEngagementAnswer_ShouldThrowException_WhenAnswerIsNullOrWhiteSpace(string? badJson)
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _sessionService.UpsertPreEngagementAnswer(questionId, taskId, badJson!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UpsertPreEngagementAnswer_ShouldNotUpdate_WhenJsonIsEmptyObject()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var emptyJson = "{}";

        // Act
        _sessionService.UpsertPreEngagementAnswer(questionId, taskId, emptyJson);

        // Assert
        _sessionMock.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void UpsertPreEngagementAnswer_ShouldNotSet_WhenAnswerUnchanged()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var answerJson = "{\"value\":\"same\"}";

        var existing = new List<PreEngagementAnswer>
        {
            new PreEngagementAnswer { QuestionId = questionId, TaskId = taskId, AnswerJson = answerJson }
        };

        var serialized = JsonConvert.SerializeObject(existing);
        var sessionBytes = Encoding.UTF8.GetBytes(serialized);

        _sessionMock.Setup(s => s.TryGetValue(SessionKeys.PreEngagementAnswers, out sessionBytes))
            .Returns(true);

        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Verifiable();

        // Act
        _sessionService.UpsertPreEngagementAnswer(questionId, taskId, answerJson);

        // Assert
        _sessionMock.Verify(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }
}