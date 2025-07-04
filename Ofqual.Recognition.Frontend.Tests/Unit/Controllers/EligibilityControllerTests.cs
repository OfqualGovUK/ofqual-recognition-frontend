using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class EligibilityControllerTests
{
    private readonly Mock<IEligibilityService> _eligibilityServiceMock = new();
    private readonly Mock<ISessionService> _sessionServiceMock = new();
    private readonly EligibilityController _controller;

    public EligibilityControllerTests()
    {
        _controller = new EligibilityController(_eligibilityServiceMock.Object, _sessionServiceMock.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Start_Get_ReturnsView()
    {
        // Act
        var result = _controller.Start();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void QuestionOne_Get_ReturnsViewWithModel()
    {
        // Arrange
        var question = new EligibilityQuestion { Answer = "Some answer" };
        _eligibilityServiceMock.Setup(x => x.GetQuestion(SessionKeys.EligibilityQuestionOne))
            .Returns(question);

        // Act
        var result = _controller.QuestionOne(It.IsAny<string>());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionOneViewModel>(viewResult.Model);
        Assert.Equal("Some answer", model.Answer);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void QuestionOne_Post_InvalidModelState_ReturnsSameView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Answer", "Required");
        var viewModel = new QuestionOneViewModel();

        // Act
        var result = _controller.QuestionOne(viewModel, returnUrl: null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(viewModel, viewResult.Model);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(null)]
    [InlineData("/some-return-url")]
    public void QuestionOne_Post_ValidModelState_RedirectsProperly(string? returnUrl)
    {
        // Arrange
        var viewModel = new QuestionOneViewModel { Answer = "Yes" };

        // Act
        var result = _controller.QuestionOne(viewModel, returnUrl);

        // Assert
        _sessionServiceMock.Verify(x => x.SetInSession(SessionKeys.EligibilityQuestionOne, "Yes"), Times.Once);

        if (string.IsNullOrEmpty(returnUrl))
        {
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("QuestionTwo", redirectResult.ActionName);
        }
        else
        {
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(returnUrl, redirectResult.Url);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void QuestionTwo_Get_ReturnsViewWithModel()
    {
        // Arrange
        var question = new EligibilityQuestion { Answer = "Answer2" };
        _eligibilityServiceMock.Setup(x => x.GetQuestion(SessionKeys.EligibilityQuestionTwo))
            .Returns(question);

        // Act
        var result = _controller.QuestionTwo(It.IsAny<string>());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionTwoViewModel>(viewResult.Model);
        Assert.Equal("Answer2", model.Answer);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(null)]
    [InlineData("/another-return-url")]
    public void QuestionTwo_Post_ValidAndInvalidModelState(string? returnUrl)
    {
        // Invalid case
        var invalidModel = new QuestionTwoViewModel();
        _controller.ModelState.AddModelError("Answer", "Required");
        var invalidResult = _controller.QuestionTwo(invalidModel, returnUrl);
        var invalidView = Assert.IsType<ViewResult>(invalidResult);
        Assert.Equal(invalidModel, invalidView.Model);

        // Valid case
        _controller.ModelState.Clear();
        var validModel = new QuestionTwoViewModel { Answer = "Yes" };
        var validResult = _controller.QuestionTwo(validModel, returnUrl);
        _sessionServiceMock.Verify(x => x.SetInSession(SessionKeys.EligibilityQuestionTwo, "Yes"), Times.Once);

        if (string.IsNullOrEmpty(returnUrl))
        {
            var redirectResult = Assert.IsType<RedirectToActionResult>(validResult);
            Assert.Equal("QuestionThree", redirectResult.ActionName);
        }
        else
        {
            var redirectResult = Assert.IsType<RedirectResult>(validResult);
            Assert.Equal(returnUrl, redirectResult.Url);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void QuestionThree_Get_ReturnsViewWithModel()
    {
        // Arrange
        var question = new EligibilityQuestion { Answer = "Answer3" };

        _eligibilityServiceMock.Setup(x => x.GetQuestion(SessionKeys.EligibilityQuestionThree))
            .Returns(question);

        // Act
        var result = _controller.QuestionThree(It.IsAny<string>());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionThreeViewModel>(viewResult.Model);
        Assert.Equal("Answer3", model.Answer);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void QuestionThree_Post_InvalidModelState_ReturnsSameView()
    {
        // Arrange
        _controller.ModelState.AddModelError("Answer", "Required");
        var viewModel = new QuestionThreeViewModel();

        // Act
        var result = _controller.QuestionThree(viewModel, It.IsAny<string>());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(viewModel, viewResult.Model);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(null)]
    [InlineData("/another-return-url")]
    public void QuestionThree_Post_ValidModelState_RedirectsBasedOnReturnUrl(string? returnUrl)
    {
        // Arrange
        var viewModel = new QuestionThreeViewModel { Answer = "Yes" };

        // Act
        var result = _controller.QuestionThree(viewModel, returnUrl);

        // Assert
        _sessionServiceMock.Verify(x => x.SetInSession(SessionKeys.EligibilityQuestionThree, "Yes"), Times.Once);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(returnUrl, redirectResult.Url);
        }
        else
        {
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("QuestionReview", redirectResult.ActionName);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void QuestionCheck_Get_ReturnsViewWithEligibilityModel()
    {
        // Arrange
        var eligibility = new Eligibility
        {
            QuestionOne = "Yes",
            QuestionTwo = "No",
            QuestionThree = "Yes"
        };

        _eligibilityServiceMock.Setup(x => x.GetAnswers())
            .Returns(eligibility);

        // Act
        var result = _controller.QuestionReview();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EligibilityViewModel>(viewResult.Model);
        Assert.Equal("Yes", model.QuestionOne);
        Assert.Equal("No", model.QuestionTwo);
        Assert.Equal("Yes", model.QuestionThree);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("No", "Yes", "Yes", "NotEligible")]
    [InlineData("Yes", "Yes", "Yes", "Eligible")]
    public void QuestionSubmit_Post_RedirectsBasedOnEligibility(string answer1, string answer2, string answer3, string expectedAction)
    {
        // Arrange
        var eligibility = new Eligibility
        {
            QuestionOne = answer1,
            QuestionTwo = answer2,
            QuestionThree = answer3
        };

        _eligibilityServiceMock.Setup(x => x.GetAnswers())
            .Returns(eligibility);

        // Act
        var result = _controller.QuestionSubmit();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(expectedAction, redirectResult.ActionName);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("No", "No", "No", "Start")]
    [InlineData("Yes", "Yes", "Yes", null)]
    public void Eligible_Get_RedirectsOrReturnsView(string answer1, string answer2, string answer3, string? expectedRedirectAction)
    {
        // Arrange
        var eligibility = new Eligibility
        {
            QuestionOne = answer1,
            QuestionTwo = answer2,
            QuestionThree = answer3
        };

        _eligibilityServiceMock.Setup(x => x.GetAnswers())
            .Returns(eligibility);

        // Act
        var result = _controller.Eligible();

        // Assert
        if (!string.IsNullOrEmpty(expectedRedirectAction))
        {
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(expectedRedirectAction, redirectResult.ActionName);
        }
        else
        {
            Assert.IsType<ViewResult>(result);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NotEligible_Get_ReturnsViewWithEligibilityModel()
    {
        // Arrange
        var eligibility = new Eligibility
        {
            QuestionOne = "No",
            QuestionTwo = "Yes",
            QuestionThree = "No"
        };

        _eligibilityServiceMock.Setup(x => x.GetAnswers())
            .Returns(eligibility);

        // Act
        var result = _controller.NotEligible();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EligibilityViewModel>(viewResult.Model);
        Assert.Equal("No", model.QuestionOne);
        Assert.Equal("Yes", model.QuestionTwo);
        Assert.Equal("No", model.QuestionThree);
    }
}
