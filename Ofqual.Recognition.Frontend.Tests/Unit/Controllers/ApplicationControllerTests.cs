using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.Extensions.Primitives;
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
    private readonly Mock<IAttachmentService> _attachmentServiceMock;
    private readonly ApplicationController _controller;

    public ApplicationControllerTests()
    {
        _applicationServiceMock = new Mock<IApplicationService>();
        _taskServiceMock = new Mock<ITaskService>();
        _sessionServiceMock = new Mock<ISessionService>();
        _questionServiceMock = new Mock<IQuestionService>();
        _attachmentServiceMock = new Mock<IAttachmentService>();

        _controller = new ApplicationController(_applicationServiceMock.Object, _taskServiceMock.Object, _sessionServiceMock.Object, _questionServiceMock.Object, _attachmentServiceMock.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InitialiseApplication_ReturnsRedirectToTaskList_WhenApplicationIsNotNull()
    {
        // Arrange
        var application = new Application
        {
            ApplicationId = Guid.NewGuid()
        };

        _applicationServiceMock.Setup(x => x.InitialiseApplication())
            .ReturnsAsync(application);

        // Act
        var result = await _controller.InitialiseApplication();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaskList", redirectResult.ActionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task InitialiseApplication_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _applicationServiceMock.Setup(x => x.InitialiseApplication())
            .ReturnsAsync((Application?)null);

        // Act
        var result = await _controller.InitialiseApplication();

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
                        Status = StatusType.NotStarted,
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
        Assert.Equal(StatusType.NotStarted, task.Status);
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
            .Returns(StatusType.Completed);

        // Act
        var result = await _controller.QuestionDetails("task", "question");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaskReview", redirect.ActionName);
        Assert.Equal("task", redirect.RouteValues!["taskNameUrl"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task QuestionDetails_ReturnsViewResult_WhenDataIsValid()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var answer = new QuestionAnswer { Answer = "{\"text\":\"sample\"}" };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _questionServiceMock.Setup(x => x.GetQuestionAnswer(application.ApplicationId, question.QuestionId))
            .ReturnsAsync(answer);

        _sessionServiceMock.Setup(x => x.GetTaskStatusFromSession(question.TaskId))
            .Returns(StatusType.InProgress);

        _attachmentServiceMock.Setup(x => x.GetAllLinkedFiles(LinkType.Question, question.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());

        // Act
        var result = await _controller.QuestionDetails("task", "question");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.Equal(question.QuestionId, model.QuestionId);
        Assert.False(model.FromReview);
        Assert.Equal(answer.Answer, model.AnswerJson);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task QuestionDetails_Sets_FromReview_True_WhenProvided()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _questionServiceMock.Setup(x => x.GetQuestionAnswer(application.ApplicationId, question.QuestionId))
            .ReturnsAsync(new QuestionAnswer { Answer = null });

        _sessionServiceMock.Setup(x => x.GetTaskStatusFromSession(question.TaskId))
            .Returns(StatusType.Completed);

        _attachmentServiceMock.Setup(x => x.GetAllLinkedFiles(LinkType.Question, question.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());

        // Act
        var result = await _controller.QuestionDetails("task", "question", fromReview: true);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.True(model.FromReview);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostQuestionDetails_Should_RedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);
        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.QuestionDetails("task", "question", formData);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostQuestionDetails_Should_ReturnNotFound_WhenQuestionIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);
        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.QuestionDetails("task", "question", formData);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostQuestionDetails_Should_ReturnBadRequest_WhenNextQuestionUrlIsInvalid()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = "not-a-valid-url"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.QuestionDetails("task", "question", formData);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostQuestionDetails_Should_SkipSubmission_AndRedirect_WhenAnswersAreSame()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = "nextTask/nextQuestion"
        };
        var existingAnswer = new QuestionAnswer { Answer = "{\"key\":\"value\"}" };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _questionServiceMock.Setup(x => x.GetQuestionAnswer(application.ApplicationId, question.QuestionId)).ReturnsAsync(existingAnswer);

        var formData = new FormCollection(new Dictionary<string, StringValues> { { "key", "value" } });

        // Act
        var result = await _controller.QuestionDetails("task", "question", formData);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QuestionDetails", redirect.ActionName);
        Assert.Equal("nextTask", redirect.RouteValues!["taskNameUrl"]);
        Assert.Equal("nextQuestion", redirect.RouteValues["questionNameUrl"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostQuestionDetails_Should_ReturnViewWithErrors_WhenValidationFails()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current"
        };
        var validationResponse = new ValidationResponse
        {
            Errors = new List<ValidationErrorItem>
            {
                new() { PropertyName = "field", ErrorMessage = "Error message" }
            }
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _questionServiceMock.Setup(x => x.GetQuestionAnswer(application.ApplicationId, question.QuestionId)).ReturnsAsync((QuestionAnswer?)null);
        _questionServiceMock.Setup(x => x.SubmitQuestionAnswer(application.ApplicationId, question.TaskId, question.QuestionId, It.IsAny<string>()))
            .ReturnsAsync(validationResponse);

        var formData = new FormCollection(new Dictionary<string, StringValues> { { "field", "value" } });

        // Act
        var result = await _controller.QuestionDetails("task", "question", formData);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<QuestionViewModel>(viewResult.Model);
        Assert.NotNull(model.Validation!.Errors);
        Assert.Contains(model.Validation.Errors, e => e.PropertyName == "field" && e.ErrorMessage == "Error message");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostQuestionDetails_Should_ReturnBadRequest_WhenValidationResponseIsNull()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = "nextTask/nextQuestion"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _questionServiceMock.Setup(x => x.GetQuestionAnswer(application.ApplicationId, question.QuestionId)).ReturnsAsync((QuestionAnswer?)null);
        _questionServiceMock.Setup(x => x.SubmitQuestionAnswer(application.ApplicationId, question.TaskId, question.QuestionId, It.IsAny<string>()))
            .ReturnsAsync((ValidationResponse?)null);

        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.QuestionDetails("task", "question", formData);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostQuestionDetails_Should_SubmitAnswer_AndRedirectToNextQuestion()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = "nextTask/nextQuestion"
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _questionServiceMock.Setup(x => x.GetQuestionAnswer(application.ApplicationId, question.QuestionId)).ReturnsAsync((QuestionAnswer?)null);
        _questionServiceMock.Setup(x => x.SubmitQuestionAnswer(application.ApplicationId, question.TaskId, question.QuestionId, It.IsAny<string>()))
            .ReturnsAsync(new ValidationResponse { Errors = Enumerable.Empty<ValidationErrorItem>() });

        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.QuestionDetails("task", "question", formData);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QuestionDetails", redirect.ActionName);
        Assert.Equal("nextTask", redirect.RouteValues!["taskNameUrl"]);
        Assert.Equal("nextQuestion", redirect.RouteValues["questionNameUrl"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostQuestionDetails_Should_SubmitAnswer_AndRedirectToReview_WhenNoNextQuestion()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = null
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _questionServiceMock.Setup(x => x.GetQuestionAnswer(application.ApplicationId, question.QuestionId)).ReturnsAsync((QuestionAnswer?)null);
        _questionServiceMock.Setup(x => x.SubmitQuestionAnswer(application.ApplicationId, question.TaskId, question.QuestionId, It.IsAny<string>()))
            .ReturnsAsync(new ValidationResponse { Errors = Enumerable.Empty<ValidationErrorItem>() });

        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.QuestionDetails("task", "question", formData);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaskReview", redirect.ActionName);
        Assert.Equal("task", redirect.RouteValues!["taskNameUrl"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Get_ReturnsView_WhenEverythingIsValid()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var taskId = Guid.NewGuid();

        var taskItem = new TaskDetails
        {
            TaskId = taskId,
            TaskName = "task",
            TaskNameUrl = "task",
            TaskOrderNumber = 5,
            SectionId = Guid.NewGuid()
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _taskServiceMock.Setup(x => x.GetTaskDetailsByTaskNameUrl("task-name"))
            .ReturnsAsync(taskItem);

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

        _sessionServiceMock.Setup(x => x.GetTaskStatusFromSession(taskId))
            .Returns(StatusType.Completed);

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
    public async Task TaskReview_Get_ReturnsNotFound_WhenTaskDetailsIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _taskServiceMock.Setup(x => x.GetTaskDetailsByTaskNameUrl("task-name"))
            .ReturnsAsync((TaskDetails?)null);

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
        var taskId = Guid.NewGuid();

        var taskItem = new TaskDetails
        {
            TaskId = taskId,
            TaskName = "task",
            TaskNameUrl = "task",
            TaskOrderNumber = 5,
            SectionId = Guid.NewGuid()
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(app);

        _taskServiceMock.Setup(x => x.GetTaskDetailsByTaskNameUrl("task-name"))
            .ReturnsAsync(taskItem);

        _questionServiceMock.Setup(x => x.GetTaskQuestionAnswers(app.ApplicationId, taskId))
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
        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        var model = new TaskReviewViewModel { Answer = StatusType.Completed };

        // Act
        var result = await _controller.TaskReview("task", model);

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData((StatusType)999)]
    public async Task TaskReview_Post_InvalidAnswer_ReturnsBadRequest(StatusType answer)
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var taskId = Guid.NewGuid();

        var taskItem = new TaskDetails
        {
            TaskId = taskId,
            TaskName = "task",
            TaskNameUrl = "task",
            TaskOrderNumber = 5,
            SectionId = Guid.NewGuid(),
            Stage = StageType.MainApplication,
            Status = StatusType.NotStarted
        };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _taskServiceMock
            .Setup(x => x.GetTaskDetailsByTaskNameUrl("task"))
            .ReturnsAsync(taskItem);

        var model = new TaskReviewViewModel { Answer = answer };

        // Act
        var result = await _controller.TaskReview("task", model);

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

        var taskItem = new TaskDetails
        {
            TaskId = taskId,
            TaskName = "task",
            TaskNameUrl = "task",
            TaskOrderNumber = 5,
            SectionId = Guid.NewGuid(),
            Stage = StageType.MainApplication,
            Status = StatusType.NotStarted
        };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _taskServiceMock
            .Setup(x => x.GetTaskDetailsByTaskNameUrl("task"))
            .ReturnsAsync(taskItem);

        _taskServiceMock
            .Setup(x => x.UpdateTaskStatus(application.ApplicationId, taskId, StatusType.InProgress))
            .ReturnsAsync(false);

        var model = new TaskReviewViewModel { Answer = StatusType.Completed };

        // Act
        var result = await _controller.TaskReview("task", model);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task TaskReview_Post_Submitted_True_RedirectsToConfirm()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var taskId = Guid.NewGuid();

        var taskItem = new TaskDetails
        {
            TaskId = taskId,
            TaskName = "task",
            TaskNameUrl = "task",
            TaskOrderNumber = 5,
            SectionId = Guid.NewGuid(),
            Stage = StageType.Declaration,
            Status = StatusType.NotStarted
        };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _taskServiceMock
            .Setup(x => x.GetTaskDetailsByTaskNameUrl("task"))
            .ReturnsAsync(taskItem);

        _taskServiceMock
            .Setup(x => x.UpdateTaskStatus(application.ApplicationId, taskId, StatusType.InProgress))
            .ReturnsAsync(true);

        _applicationServiceMock
            .Setup(x => x.SubmitApplication(application.ApplicationId))
            .ReturnsAsync(new Application
            {
                ApplicationId = application.ApplicationId,
                Submitted = true
            });

        var model = new TaskReviewViewModel { Answer = StatusType.Completed };

        // Act
        var result = await _controller.TaskReview("task", model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(_controller.ConfirmSubmission), redirect.ActionName);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(StatusType.Completed)]
    [InlineData(StatusType.InProgress)]
    public async Task TaskReview_Post_ValidAnswer_UpdatesStatusAndRedirects(StatusType answer)
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var taskId = Guid.NewGuid();

        var taskItem = new TaskDetails
        {
            TaskId = taskId,
            TaskName = "task",
            TaskNameUrl = "task",
            TaskOrderNumber = 5,
            SectionId = Guid.NewGuid(),
            Stage = StageType.MainApplication,
            Status = StatusType.NotStarted
        };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _taskServiceMock
            .Setup(x => x.GetTaskDetailsByTaskNameUrl("task"))
            .ReturnsAsync(taskItem);

        _taskServiceMock
            .Setup(x => x.UpdateTaskStatus(application.ApplicationId, taskId, StatusType.InProgress))
            .ReturnsAsync(true);

        var model = new TaskReviewViewModel { Answer = answer };

        // Act
        var result = await _controller.TaskReview("task", model);

        // Assert
        _taskServiceMock.Verify(x => x.UpdateTaskStatus(application.ApplicationId, taskId, StatusType.InProgress), Times.Once);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.ApplicationConstants.TASK_LIST_PATH, redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfirmSubmission_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = _controller.ConfirmSubmission();

        // Assert
        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal(RouteConstants.HomeConstants.HOME_PATH, redirect.Url);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfirmSubmission_ReturnsBadRequest_WhenApplicationNotSubmitted()
    {
        // Arrange
        var application = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Submitted = false
        };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        // Act
        var result = _controller.ConfirmSubmission();

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConfirmSubmission_ReturnsView_WhenApplicationIsSubmitted()
    {
        // Arrange
        var application = new Application
        {
            ApplicationId = Guid.NewGuid(),
            Submitted = true
        };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        // Act
        var result = _controller.ConfirmSubmission();

        // Assert
        Assert.IsType<ViewResult>(result);
    }
}