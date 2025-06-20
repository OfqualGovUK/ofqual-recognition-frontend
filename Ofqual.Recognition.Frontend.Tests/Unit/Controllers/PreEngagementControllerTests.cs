using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Models;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class PreEngagementControllerTests
{
    private readonly Mock<IPreEngagementService> _preEngagementServiceMock;
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly PreEngagementController _controller;

    public PreEngagementControllerTests()
    {
        _preEngagementServiceMock = new Mock<IPreEngagementService>();
        _sessionServiceMock = new Mock<ISessionService>();

        _controller = new PreEngagementController(_preEngagementServiceMock.Object, _sessionServiceMock.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task StartPreEngagement_Should_RedirectToFirstQuestion_WhenExists()
    {
        // Arrange
        var question = new PreEngagementQuestion
        {
            CurrentTaskNameUrl = "task1",
            CurrentQuestionNameUrl = "question1"
        };

        _preEngagementServiceMock.Setup(x => x.GetFirstPreEngagementQuestion()).ReturnsAsync(question);

        // Act
        var result = await _controller.StartPreEngagement();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("PreEngagementQuestionDetails", redirect.ActionName);
        Assert.Equal("task1", redirect.RouteValues!["taskNameUrl"]);
        Assert.Equal("question1", redirect.RouteValues["questionNameUrl"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task StartPreEngagement_Should_ReturnNotFound_WhenFirstQuestionIsNull()
    {
        // Arrange
        _preEngagementServiceMock.Setup(x => x.GetFirstPreEngagementQuestion()).ReturnsAsync((PreEngagementQuestion?)null);

        // Act
        var result = await _controller.StartPreEngagement();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PreEngagementQuestionDetails_Should_ReturnView_WithMappedViewModel_WhenQuestionExists()
    {
        // Arrange
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var preAnswers = new List<PreEngagementAnswer>
        {
            new PreEngagementAnswer
            {
                QuestionId = questionDetails.QuestionId,
                TaskId = questionDetails.TaskId,
                AnswerJson = "{\"test\":\"answer\"}"
            }
        };

        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task", "question"))
            .ReturnsAsync(questionDetails);

        _sessionServiceMock.Setup(x => x.GetFromSession<List<PreEngagementAnswer>>(SessionKeys.PreEngagementAnswers))
            .Returns(preAnswers);

        // Act
        var result = await _controller.PreEngagementQuestionDetails("task", "question");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.Equal("{\"test\":\"answer\"}", model.AnswerJson);
        Assert.True(model.FromPreEngagement);
        Assert.Equal("~/Views/Application/QuestionDetails.cshtml", viewResult.ViewName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PreEngagementQuestionDetails_Should_ReturnNotFound_WhenQuestionIsNull()
    {
        // Arrange
        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.PreEngagementQuestionDetails("task", "question");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostPreEngagementQuestionDetails_Should_StoreAnswer_AndRedirectToNext_WhenNextExists()
    {
        // Arrange
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = "task2/question2",
            QuestionTypeName = "textinput"
        };

        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync(questionDetails);

        _preEngagementServiceMock.Setup(x => x.ValidatePreEngagementAnswer(questionDetails.QuestionId, It.IsAny<string>()))
            .ReturnsAsync((ValidationResponse?)null);

        var formData = new FormCollection(new Dictionary<string, StringValues> { { "field", "value" } });

        // Act
        var result = await _controller.PreEngagementQuestionDetails("task1", "question1", formData);

        // Assert
        var badRequest = Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostPreEngagementQuestionDetails_Should_RedirectToStartApplication_WhenNextIsNull()
    {
        // Arrange
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionContent = "{}",
            NextQuestionUrl = null,
            CurrentQuestionUrl = "current-url",
            QuestionTypeName = "textinput"
        };

        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync(questionDetails);

        _preEngagementServiceMock.Setup(x => x.ValidatePreEngagementAnswer(questionDetails.QuestionId, It.IsAny<string>()))
            .ReturnsAsync(new ValidationResponse { Errors = Enumerable.Empty<ValidationErrorItem>() });

        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.PreEngagementQuestionDetails("task1", "question1", formData);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("StartApplication", redirect.ActionName);
        Assert.Equal("Application", redirect.ControllerName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostPreEngagementQuestionDetails_Should_ReturnBadRequest_WhenNextUrlInvalid()
    {
        // Arrange
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionContent = "{}",
            NextQuestionUrl = "invalid_url",
            CurrentQuestionUrl = "current-url",
            QuestionTypeName = "textinput"
        };

        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync(questionDetails);

        _preEngagementServiceMock.Setup(x => x.ValidatePreEngagementAnswer(questionDetails.QuestionId, It.IsAny<string>()))
            .ReturnsAsync(new ValidationResponse { Errors = Enumerable.Empty<ValidationErrorItem>() });

        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.PreEngagementQuestionDetails("task1", "question1", formData);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid next question URL.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostPreEngagementQuestionDetails_Should_ReturnNotFound_WhenQuestionDetailsNull()
    {
        // Arrange
        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync((QuestionDetails?)null);

        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.PreEngagementQuestionDetails("task1", "question1", formData);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PostPreEngagementQuestionDetails_Should_ReturnView_WhenValidationFails()
    {
        // Arrange
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = "task2/question2",
            QuestionTypeName = "textinput"
        };

        var validationResponse = new ValidationResponse
        {
            Errors = new List<ValidationErrorItem>
            {
                new ValidationErrorItem { PropertyName = "field", ErrorMessage = "Field is required." }
            }
        };

        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync(questionDetails);

        _preEngagementServiceMock.Setup(x => x.ValidatePreEngagementAnswer(questionDetails.QuestionId, It.IsAny<string>()))
            .ReturnsAsync(validationResponse);

        var formData = new FormCollection(new Dictionary<string, StringValues> { { "field", "value" } });

        // Act
        var result = await _controller.PreEngagementQuestionDetails("task1", "question1", formData);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Application/QuestionDetails.cshtml", viewResult.ViewName);

        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.Single(model.Validation!.Errors!);
        Assert.Equal("Field is required.", model.Validation!.Errors!.First().ErrorMessage);
    }
}