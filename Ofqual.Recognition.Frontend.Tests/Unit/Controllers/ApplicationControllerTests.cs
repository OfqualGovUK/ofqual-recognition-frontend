using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Ofqual.Recognition.Frontend.Web.ViewModels;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class ApplicationControllerTests
{
    private readonly Mock<IApplicationService> _applicationServiceMock;
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly Mock<IQuestionService> _questionServiceMock;
    private readonly ApplicationController _controller;

    public ApplicationControllerTests()
    {
        _applicationServiceMock = new Mock<IApplicationService>();
        _taskServiceMock = new Mock<ITaskService>();
        _sessionServiceMock = new Mock<ISessionService>();
        _questionServiceMock = new Mock<IQuestionService>();

        _controller = new ApplicationController(_applicationServiceMock.Object, _taskServiceMock.Object, _sessionServiceMock.Object, _questionServiceMock.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task StartApplication_ReturnsRedirectToTaskList_WhenApplicationIsNotNull()
    {
        // Arrange
        var application = new Application
        {
            ApplicationId = Guid.NewGuid()
        };

        _applicationServiceMock.Setup(x => x.SetUpApplication())
            .ReturnsAsync(application);

        // Act
        var result = await _controller.StartApplication();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaskList", redirectResult.ActionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task StartApplication_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _applicationServiceMock.Setup(x => x.SetUpApplication())
            .ReturnsAsync((Application?)null);

        // Act
        var result = await _controller.StartApplication();

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirectResult.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskList_ReturnsViewWithMappedViewModel_WhenApplicationIsInSession()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var application = new Application { ApplicationId = applicationId };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        var domainTasks = new List<TaskItemStatusSection>
        {
            new TaskItemStatusSection
            {
                SectionId = Guid.NewGuid(),
                SectionName = "Personal Info",
                Tasks = new List<TaskItemStatus>
                {
                    new TaskItemStatus
                    {
                        TaskId = Guid.NewGuid(),
                        TaskName = "Your name",
                        Status = TaskStatusEnum.NotStarted,
                        FirstQuestionURL = "/application-details/contact-details"
                    }
                }
            }
        };

        _taskServiceMock
            .Setup(x => x.GetApplicationTasks(applicationId))
            .ReturnsAsync(domainTasks);

        // Act
        var result = await _controller.TaskList();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TaskListViewModel>(viewResult.Model);

        Assert.Single(model.Sections);
        var section = model.Sections.First();
        Assert.Equal("Personal Info", section.SectionName);
        Assert.Single(section.Tasks);

        var task = section.Tasks.First();
        Assert.Equal("Your name", task.TaskName);
        Assert.Equal(TaskStatusEnum.NotStarted, task.Status);
        Assert.True(task.IsLink);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskList_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.TaskList();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TaskReview_Get_ReturnsView_WhenApplicationIsInSession()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        // Act
        var result = _controller.TaskReview();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TaskReview_Get_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = _controller.TaskReview();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(TaskStatusEnum.Completed)]
    [InlineData(TaskStatusEnum.InProgress)]
    public async Task TaskReview_Post_ValidAnswer_UpdatesStatusAndRedirects(TaskStatusEnum answer)
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var model = new TaskReviewViewModel { Answer = answer };

        var application = new Application { ApplicationId = Guid.NewGuid() };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        // Act
        var result = await _controller.SubmitTaskReview(taskId, model);

        // Assert
        _taskServiceMock.Verify(x => x.UpdateTaskStatus(application.ApplicationId, taskId, answer), Times.Once);
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.ApplicationConstants.TASK_LIST_PATH, redirect.Url);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData((TaskStatusEnum)999)] // Invalid enum
    public async Task TaskReview_Post_InvalidAnswer_RedirectsToSameView(TaskStatusEnum answer)
    {
        // Arrange
        var taskId = Guid.NewGuid();

        var model = new TaskReviewViewModel { Answer = answer };
        var application = new Application { ApplicationId = Guid.NewGuid() };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        // Act
        var result = await _controller.SubmitTaskReview(taskId, model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaskReview", redirect.ActionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Post_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        var model = new TaskReviewViewModel { Answer = TaskStatusEnum.Completed };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.SubmitTaskReview(Guid.NewGuid(), model);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task QuestionDetails_ReturnsViewResult_WhenDataIsValid()
    {
        // Arrange
        var taskName = "Task1";
        var questionName = "Question1";
        var mockApplication = new Application();

        var mockQuestionResponse = new QuestionResponse
        {
            QuestionId = Guid.NewGuid(),
            QuestionTypeName = "Multiple Choice",
            QuestionContent = "{\"hint\":\"test.\"}",
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(It.IsAny<string>()))
            .Returns(mockApplication);

        _questionServiceMock.Setup(q => q.GetQuestionDetails(taskName, questionName))
            .ReturnsAsync(mockQuestionResponse);

        // Act
        var result = await _controller.QuestionDetails(taskName, questionName);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);

        Assert.Equal(mockQuestionResponse.QuestionTypeName, model.QuestionTypeName);
        Assert.Equal(mockQuestionResponse.QuestionId, model.QuestionId);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QuestionDetails_ReturnsExpectedResult_WhenDataIsMissing(bool isApplicationNull)
    {
        // Arrange
        if (isApplicationNull)
        {
            _sessionServiceMock.Setup(s => s.GetFromSession<Application>(It.IsAny<string>()))
                .Returns((Application?)null);
        }
        else
        {
            _sessionServiceMock.Setup(s => s.GetFromSession<Application>(It.IsAny<string>()))
                .Returns(new Application());
            _questionServiceMock.Setup(q => q.GetQuestionDetails(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((QuestionResponse?)null);
        }

        // Act
        var result = await _controller.QuestionDetails("task", "question");

        // Assert
        if (isApplicationNull)
        {
            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
        }
        else
        {
            Assert.IsType<NotFoundResult>(result);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitAnswers_Should_Redirect_To_NextQuestionUrl_When_Valid()
    {
        // Arrange
        var taskName = "criteria-a";
        var questionName = "criteria-a6-2";
        var applicationId = Guid.NewGuid();
        var questionId = Guid.NewGuid();

        var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { "someField", "someValue" },
            { "__RequestVerificationToken", "token" }
        });

        var application = new Application { ApplicationId = applicationId };
        var question = new QuestionResponse { QuestionId = questionId };
        var nextUrl = "criteria-a/criteria-a6-3";
        var answerResult = new QuestionAnswerResult { NextQuestionUrl = nextUrl };

        _sessionServiceMock
            .Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _sessionServiceMock
            .Setup(s => s.GetFromSession<QuestionResponse>($"{taskName}/{questionName}"))
            .Returns(question);

        _questionServiceMock
            .Setup(q => q.SubmitQuestionAnswer(applicationId, questionId, It.IsAny<string>()))
            .ReturnsAsync(answerResult);

        // Act
        var result = await _controller.SubmitAnswers(taskName, questionName, form);

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal($"/application/{nextUrl}", redirectResult.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitAnswers_Should_Return_NotFound_When_Question_Is_Null()
    {
        // Arrange
        _sessionServiceMock
            .Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _sessionServiceMock
            .Setup(s => s.GetFromSession<QuestionResponse>("criteria-a/criteria-a6-2"))
            .Returns((QuestionResponse?)null);

        // Act
        var result = await _controller.SubmitAnswers("criteria-a", "criteria-a6-2", new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()));

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("criteria-a/criteria-a6-2", redirect.ActionName);
    }

    [Fact]
    public async Task SubmitAnswers_Should_Redirect_To_Home_If_Application_Is_Null()
    {
        // Arrange
        _sessionServiceMock
            .Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);
        // Act
        var result = await _controller.SubmitAnswers("criteria-a", "criteria-a6-2", new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()));

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }
}
