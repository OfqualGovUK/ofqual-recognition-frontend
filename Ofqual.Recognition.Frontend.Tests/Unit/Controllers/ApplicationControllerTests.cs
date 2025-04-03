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
    private readonly ApplicationController _controller;

    public ApplicationControllerTests()
    {
        _applicationServiceMock = new Mock<IApplicationService>();
        _taskServiceMock = new Mock<ITaskService>();
        _sessionServiceMock = new Mock<ISessionService>();
        _controller = new ApplicationController(_applicationServiceMock.Object, _taskServiceMock.Object, _sessionServiceMock.Object);
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
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirectResult.ActionName);
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
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirect.ActionName);
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
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirect.ActionName);
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
        var result = await _controller.TaskReview(taskId, model);

        // Assert
        _taskServiceMock.Verify(x => x.UpdateTaskStatus(application.ApplicationId, taskId, answer), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaskList", redirect.ActionName);
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
        var result = await _controller.TaskReview(taskId, model);

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
        var result = await _controller.TaskReview(Guid.NewGuid(), model);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirect.ActionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ApplicationReview_ReturnsView_WhenApplicationIsInSession()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        // Act
        var result = _controller.ApplicationReview();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ApplicationReview_ReturnsRedirectToHome_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = _controller.ApplicationReview();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Home", redirect.ActionName);
    }
}
