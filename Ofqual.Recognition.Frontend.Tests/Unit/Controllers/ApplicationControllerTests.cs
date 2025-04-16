using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
    public async Task QuestionDetails_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.QuestionDetails("task", "question");

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task QuestionDetails_ReturnsNotFound_WhenQuestionIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.QuestionDetails("task", "question");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task QuestionDetails_RedirectsToReview_WhenTaskCompletedAndNotFromReview()
    {
        // Arrange
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _sessionServiceMock.Setup(x => x.GetTaskStatusFromSession(question.TaskId))
            .Returns(TaskStatusEnum.Completed);

        // Act
        var result = await _controller.QuestionDetails("task", "question");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("TaskReview", redirect.ActionName);
        Assert.Equal("task", redirect.RouteValues["taskName"]);
        Assert.Equal("question", redirect.RouteValues["questionName"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task QuestionDetails_ReturnsViewResult_WhenDataIsValid()
    {
        // Arrange
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _sessionServiceMock.Setup(x => x.GetTaskStatusFromSession(question.TaskId))
            .Returns(TaskStatusEnum.InProgress);

        // Act
        var result = await _controller.QuestionDetails("task", "question");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);

        Assert.Equal(question.QuestionId, model.QuestionId);
        Assert.False(model.FromReview);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task QuestionDetails_Sets_FromReview_True_WhenProvided()
    {
        // Arrange
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _sessionServiceMock.Setup(x => x.GetTaskStatusFromSession(question.TaskId))
            .Returns(TaskStatusEnum.Completed);

        // Act
        var result = await _controller.QuestionDetails("task", "question", fromReview: true);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.True(model.FromReview);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitAnswers_Should_RedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.SubmitAnswers("task", "question", new FormCollection(new()));

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitAnswers_Should_ReturnNotFound_WhenQuestionIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.SubmitAnswers("task", "question", new FormCollection(new()));

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitAnswers_Should_RedirectToReview_WhenAnswerSubmissionIsNull()
    {
        // Arrange
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application { ApplicationId = Guid.NewGuid() });

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _questionServiceMock.Setup(x =>
            x.SubmitQuestionAnswer(It.IsAny<Guid>(), question.TaskId, question.QuestionId, It.IsAny<string>()))
            .ReturnsAsync((QuestionAnswerSubmissionResponse?)null);

        // Act
        var result = await _controller.SubmitAnswers("task", "question", new FormCollection(new()));

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaskReview", redirect.ActionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitAnswers_Should_ReturnNotFound_WhenNextUrlIsInvalid()
    {
        // Arrange
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application { ApplicationId = Guid.NewGuid() });

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _questionServiceMock.Setup(x =>
            x.SubmitQuestionAnswer(It.IsAny<Guid>(), question.TaskId, question.QuestionId, It.IsAny<string>()))
            .ReturnsAsync(new QuestionAnswerSubmissionResponse { NextQuestionNameUrl = "", NextTaskNameUrl = "" });

        // Act
        var result = await _controller.SubmitAnswers("task", "question", new FormCollection(new()));

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitAnswers_Should_RedirectToNextQuestion_WhenValid()
    {
        // Arrange
        var applicationId = Guid.NewGuid();
        var nextTaskUrl = "task2";
        var nextQuestionUrl = "question2";
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application { ApplicationId = applicationId });

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _questionServiceMock.Setup(x => x.SubmitQuestionAnswer(applicationId, question.TaskId, question.QuestionId, It.IsAny<string>()))
            .ReturnsAsync(new QuestionAnswerSubmissionResponse { NextQuestionNameUrl = nextQuestionUrl, NextTaskNameUrl = nextTaskUrl });

        // Act
        var result = await _controller.SubmitAnswers("task", "question", new FormCollection(new()));

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QuestionDetails", redirect.ActionName);
        Assert.Equal("task2", redirect.RouteValues["taskName"]);
        Assert.Equal("question2", redirect.RouteValues["questionName"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Get_ReturnsView_WhenEverythingIsValid()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var taskId = Guid.NewGuid();

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = Guid.NewGuid(),
                TaskId = taskId,
                QuestionTypeName = "Text",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        var answers = new List<QuestionAnswerSection>
        {
            new()
            {
                SectionHeading = "test",
                QuestionAnswers = new List<QuestionAnswerReview>
                {
                    new QuestionAnswerReview
                    {
                        QuestionText = "What is your name?",
                        AnswerValue = new List<string> { "Test" },
                        QuestionUrl = "task/q1"
                    }
                }
            }
        };
        _questionServiceMock.Setup(x => x.GetTaskQuestionAnswers(application.ApplicationId, taskId))
            .ReturnsAsync(answers);

        // Act
        var result = await _controller.TaskReview("task-name");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<TaskReviewViewModel>(viewResult.Model);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Get_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.TaskReview("task-name");

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Get_ReturnsNotFound_WhenQuestionDetailsIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.TaskReview("task-name");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Get_ReturnsNotFound_WhenReviewAnswersAreEmpty()
    {
        // Arrange
        var app = new Application { ApplicationId = Guid.NewGuid() };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(app);

        _questionServiceMock.Setup(x => x.GetQuestionDetails(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "Text",
                QuestionContent = "{}",
                CurrentQuestionUrl = "some/url"
            });

        _questionServiceMock.Setup(x => x.GetTaskQuestionAnswers(app.ApplicationId, It.IsAny<Guid>()))
            .ReturnsAsync(new List<QuestionAnswerSection>());

        // Act
        var result = await _controller.TaskReview("task-name");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Post_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        var model = new TaskReviewViewModel { Answer = TaskStatusEnum.Completed };

        // Act
        var result = await _controller.SubmitTaskReview("task", model);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData((TaskStatusEnum)999)]
    public async Task TaskReview_Post_InvalidAnswer_ReturnsBadRequest(TaskStatusEnum answer)
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "Text",
                QuestionContent = "{}",
                CurrentQuestionUrl = "some/url"
            });

        var model = new TaskReviewViewModel { Answer = answer };

        // Act
        var result = await _controller.SubmitTaskReview("task", model);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Post_ReturnsBadRequest_WhenUpdateFails()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var taskId = Guid.NewGuid();

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = Guid.NewGuid(),
                TaskId = taskId,
                QuestionTypeName = "Text",
                QuestionContent = "{}",
                CurrentQuestionUrl = "some/url"
            });

        _taskServiceMock.Setup(x => x.UpdateTaskStatus(application.ApplicationId, taskId, TaskStatusEnum.Completed))
            .ReturnsAsync(false);

        var model = new TaskReviewViewModel { Answer = TaskStatusEnum.Completed };

        // Act
        var result = await _controller.SubmitTaskReview("task", model);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(TaskStatusEnum.Completed)]
    [InlineData(TaskStatusEnum.InProgress)]
    public async Task TaskReview_Post_ValidAnswer_UpdatesStatusAndRedirects(TaskStatusEnum answer)
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var taskId = Guid.NewGuid();

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = Guid.NewGuid(),
                TaskId = taskId,
                QuestionTypeName = "Text",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        _taskServiceMock.Setup(x => x.UpdateTaskStatus(application.ApplicationId, taskId, answer))
            .ReturnsAsync(true);

        var model = new TaskReviewViewModel { Answer = answer };

        // Act
        var result = await _controller.SubmitTaskReview("task", model);

        // Assert
        _taskServiceMock.Verify(x => x.UpdateTaskStatus(application.ApplicationId, taskId, answer), Times.Once);
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.ApplicationConstants.TASK_LIST_PATH, redirect.Url);
    }
}