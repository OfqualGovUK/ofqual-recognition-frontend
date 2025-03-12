﻿using Moq;
using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers
{
    public class EligibilityControllerTests
    {
        // Helper to create the controller with a mocked service
        private EligibilityController GetController(IEligibilityService service)
        {
            return new EligibilityController(service);
        }

        [Fact]
        public void QuestionOne_Get_ReturnsViewWithModel()
        {
            // Arrange
            var eligibilityServiceMock = new Mock<IEligibilityService>();
            eligibilityServiceMock
                .Setup(x => x.GetAnswers())
                .Returns(new EligibilityModel
                {
                    QuestionOne = "Yes",
                    QuestionTwo = string.Empty,
                    QuestionThree = string.Empty
                });

            var controller = GetController(eligibilityServiceMock.Object);

            // Act
            var result = controller.QuestionOne() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as EligibilityModel;
            Assert.NotNull(model);
            Assert.Equal("Yes", model.QuestionOne);
        }

        [Fact]
        public void QuestionOne_Post_InvalidInput_ReturnsViewWithModelError()
        {
            // Arrange
            var eligibilityServiceMock = new Mock<IEligibilityService>();
            eligibilityServiceMock
                .Setup(x => x.GetAnswers())
                .Returns(new EligibilityModel
                {
                    QuestionOne = string.Empty,
                    QuestionTwo = string.Empty,
                    QuestionThree = string.Empty
                });

            var controller = GetController(eligibilityServiceMock.Object);

            // Act
            var result = controller.QuestionOne("") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.True(controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public void QuestionOne_Post_ValidInput_RedirectsToQuestionTwo()
        {
            // Arrange
            var eligibilityServiceMock = new Mock<IEligibilityService>();
            var controller = GetController(eligibilityServiceMock.Object);

            // Act
            var result = controller.QuestionOne("Yes") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("QuestionTwo", result.ActionName);
            eligibilityServiceMock.Verify(x => x.SaveAnswers("Yes", null, null), Times.Once);
        }
    }
}