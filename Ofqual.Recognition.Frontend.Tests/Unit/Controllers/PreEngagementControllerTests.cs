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
        Assert.Equal("task1", redirect.RouteValues["taskNameUrl"]);
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
    public async Task PreEngagementSubmitAnswers_Should_StoreAnswer_AndRedirectToNext_WhenNextExists()
    {
        // Arrange
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = "task2/question2"
        };

        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync(questionDetails);
        
        var formData = new FormCollection(new Dictionary<string, StringValues> { { "field", "value" } });

        // Act
        var result = await _controller.PreEngagementSubmitAnswers("task1", "question1", formData);
        
        // Assert
        _sessionServiceMock.Verify(x =>
            x.UpsertPreEngagementAnswer(questionDetails.QuestionId, questionDetails.TaskId, It.IsAny<string>()), Times.Once);
        
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("PreEngagementQuestionDetails", redirect.ActionName);
        Assert.Equal("task2", redirect.RouteValues["taskNameUrl"]);
        Assert.Equal("question2", redirect.RouteValues["questionNameUrl"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PreEngagementSubmitAnswers_Should_RedirectToStartApplication_WhenNextQuestionUrlIsNull()
    {
        // Arrange
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = null
        };

        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync(questionDetails);
        
        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.PreEngagementSubmitAnswers("task1", "question1", formData);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("StartApplication", redirect.ActionName);
        Assert.Equal("Application", redirect.ControllerName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PreEngagementSubmitAnswers_Should_ReturnBadRequest_WhenNextUrlIsInvalid()
    {
        // Arrange
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "current",
            NextQuestionUrl = "invalid_url"
        };

        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync(questionDetails);
        
        var formData = new FormCollection(new Dictionary<string, StringValues>());

        // Act
        var result = await _controller.PreEngagementSubmitAnswers("task1", "question1", formData);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid next question URL.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task PreEngagementSubmitAnswers_Should_ReturnNotFound_WhenQuestionIsNull()
    {
        // Arrange
        _preEngagementServiceMock.Setup(x => x.GetPreEngagementQuestionDetails("task1", "question1"))
            .ReturnsAsync((QuestionDetails?)null);
        
        var formData = new FormCollection(new Dictionary<string, StringValues>());
        
        // Act
        var result = await _controller.PreEngagementSubmitAnswers("task1", "question1", formData);
        
        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}