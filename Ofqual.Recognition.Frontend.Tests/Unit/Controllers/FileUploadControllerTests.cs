using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Tests.Helpers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Moq;

namespace Ofqual.Recognition.Frontend.Tests.Unit.Controllers;

public class FileUploadControllerTests
{
    private readonly Mock<ISessionService> _sessionServiceMock;
    private readonly Mock<IQuestionService> _questionServiceMock;
    private readonly Mock<IAttachmentService> _attachmentServiceMock;
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly FileUploadController _controller;

    public FileUploadControllerTests()
    {
        _sessionServiceMock = new Mock<ISessionService>();
        _questionServiceMock = new Mock<IQuestionService>();
        _attachmentServiceMock = new Mock<IAttachmentService>();
        _taskServiceMock = new Mock<ITaskService>();

        _controller = new FileUploadController(_sessionServiceMock.Object, _questionServiceMock.Object, _attachmentServiceMock.Object, _taskServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitFile_Returns_Unauthorised_When_Application_Is_Null()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.SubmitFile("task", "question", new List<IFormFile>());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitFile_Returns_NotFound_When_Question_Is_Null()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.SubmitFile("task", "question", new List<IFormFile>());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitFile_Returns_View_With_Errors_When_File_Is_Empty()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "FileUpload",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("test.pdf");

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock
            .Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _attachmentServiceMock
            .Setup(x => x.GetAllLinkedFiles(LinkType.Question, question.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());

        // Act
        var result = await _controller.SubmitFile("task", "question", new List<IFormFile> { fileMock.Object });

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.NotEmpty(model.Validation?.Errors!);
        Assert.Contains(model.Validation?.Errors!, e => e.ErrorMessage!.Contains("is empty"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitFile_Redirects_To_Review_When_No_More_Questions()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid(), Submitted = false };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "FileUpload",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question",
            NextQuestionUrl = null
        };

        var file = new FormFile(new MemoryStream(new byte[100]), 0, 100, "files", "file.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock
            .Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _attachmentServiceMock
            .Setup(x => x.GetAllLinkedFiles(LinkType.Question, question.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());

        _attachmentServiceMock
            .Setup(x => x.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file))
            .ReturnsAsync(new AttachmentDetails
            {
                AttachmentId = Guid.NewGuid(),
                FileName = "file.txt",
                FileMIMEtype = "text/plain",
                FileSize = 100
            });

        _taskServiceMock
            .Setup(x => x.UpdateTaskStatus(application.ApplicationId, question.TaskId, StatusType.InProgress))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SubmitFile("task", "question", new List<IFormFile> { file });

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TaskReview", redirect.ActionName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitFile_Returns_BadRequest_When_Status_Update_Fails()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid(), Submitted = false };
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "FileUpload",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question",
            NextQuestionUrl = "task/next-question"
        };

        var file = new FormFile(new MemoryStream(new byte[100]), 0, 100, "files", "file.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        _sessionServiceMock
            .Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock
            .Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _attachmentServiceMock
            .Setup(x => x.GetAllLinkedFiles(LinkType.Question, question.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());

        _attachmentServiceMock
            .Setup(x => x.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file))
            .ReturnsAsync(new AttachmentDetails
            {
                AttachmentId = Guid.NewGuid(),
                FileName = "file.txt",
                FileMIMEtype = "text/plain",
                FileSize = 100
            });

        _taskServiceMock
            .Setup(x => x.UpdateTaskStatus(application.ApplicationId, question.TaskId, StatusType.InProgress))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SubmitFile("task", "question", new List<IFormFile> { file });

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsUnauthorised_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.UploadFile("task", "question", null!);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsNotFound_WhenQuestionDetailsIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.UploadFile("task", "question", null!);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsBadRequest_WhenFileIsEmpty()
    {
        // Arrange
        var application = new Application();
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var file = new FormFile(Stream.Null, 0, 0, "files", "empty.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync(question);

        // Act
        var result = await _controller.UploadFile("task", "question", file);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The selected file is empty.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsBadRequest_WhenFileTooLarge()
    {
        // Arrange
        var application = new Application();
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var file = new FormFile(new MemoryStream(new byte[26 * 1024 * 1024]), 0, 26 * 1024 * 1024, "files", "large.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync(question);

        // Act
        var result = await _controller.UploadFile("task", "question", file);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("The selected file must be smaller than 25MB.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsBadRequest_WhenFileNotAllowed()
    {
        // Arrange
        var application = new Application();
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var file = new FormFile(new MemoryStream(new byte[100]), 0, 100, "files", "script.exe")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/x-msdownload"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync(question);

        // Act
        var result = await _controller.UploadFile("task", "question", file);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Unsupported file type or content.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsBadRequest_WhenDuplicateFile()
    {
        // Arrange
        var application = new Application();
        var questionId = Guid.NewGuid();

        var question = new QuestionDetails
        {
            QuestionId = questionId,
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var file = new FormFile(new MemoryStream(new byte[100]), 0, 100, "files", "file.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var existingAttachment = new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "file.txt",
            FileSize = 100,
            FileMIMEtype = "text/plain"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _attachmentServiceMock.Setup(a => a.GetAllLinkedFiles(LinkType.Question, questionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails> { existingAttachment });

        // Act
        var result = await _controller.UploadFile("task", "question", file);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("This file has already been uploaded.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsBadRequest_WhenUploadFails()
    {
        // Arrange
        var application = new Application();
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var file = new FormFile(new MemoryStream(new byte[100]), 0, 100, "files", "upload.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _attachmentServiceMock.Setup(a => a.GetAllLinkedFiles(LinkType.Question, question.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());
        _attachmentServiceMock.Setup(a => a.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file))
            .ReturnsAsync((AttachmentDetails?)null);

        // Act
        var result = await _controller.UploadFile("task", "question", file);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to upload file.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsBadRequest_WhenStatusUpdateFails()
    {
        // Arrange
        var application = new Application();
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var file = new FormFile(new MemoryStream(new byte[100]), 0, 100, "files", "status.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var attachment = new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "status.txt",
            FileMIMEtype = "text/plain",
            FileSize = 100
        };

        _sessionServiceMock
            .Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock
            .Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _attachmentServiceMock
            .Setup(a => a.GetAllLinkedFiles(LinkType.Question, question.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());

        _attachmentServiceMock
            .Setup(a => a.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file))
            .ReturnsAsync(attachment);

        _taskServiceMock
            .Setup(t => t.UpdateTaskStatus(application.ApplicationId, question.TaskId, StatusType.InProgress))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UploadFile("task", "question", file);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var application = new Application();
        var question = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "Text",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var file = new FormFile(new MemoryStream(new byte[100]), 0, 100, "files", "success.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var attachment = new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "success.txt",
            FileMIMEtype = "text/plain",
            FileSize = 100
        };

        _sessionServiceMock
            .Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock
            .Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(question);

        _attachmentServiceMock
            .Setup(a => a.GetAllLinkedFiles(LinkType.Question, question.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());

        _attachmentServiceMock
            .Setup(a => a.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file))
            .ReturnsAsync(attachment);

        _taskServiceMock
            .Setup(t => t.UpdateTaskStatus(application.ApplicationId, question.TaskId, StatusType.InProgress))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UploadFile("task", "question", file);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(attachment.AttachmentId, ok.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_ReturnsUnauthorised_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.DeleteFile("task", "question", Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_ReturnsNotFound_WhenQuestionDetailsIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.DeleteFile("task", "question", Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_ReturnsBadRequest_WhenDeletionFails()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };

        var questionDetails = new QuestionDetails
        {
            QuestionId = questionId,
            CurrentQuestionUrl = "task/question",
            QuestionTypeName = "fileUpload",
            QuestionContent = "{}"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(questionDetails);

        _attachmentServiceMock.Setup(a =>
            a.DeleteLinkedFile(LinkType.Question, questionId, attachmentId, application.ApplicationId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteFile("task", "question", attachmentId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete file.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_RedirectsToQuestionDetails_WhenHtmlRequest()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };

        var questionDetails = new QuestionDetails
        {
            QuestionId = questionId,
            CurrentQuestionUrl = "task/question",
            QuestionTypeName = "fileUpload",
            QuestionContent = "{}"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(questionDetails);

        _attachmentServiceMock.Setup(a =>
            a.DeleteLinkedFile(LinkType.Question, questionId, attachmentId, application.ApplicationId))
            .ReturnsAsync(true);

        _controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "text/html";

        // Act
        var result = await _controller.DeleteFile("task", "question", attachmentId);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("QuestionDetails", redirect.ActionName);
        Assert.Equal("Application", redirect.ControllerName);
        Assert.Equal("task", redirect.RouteValues!["taskNameUrl"]);
        Assert.Equal("question", redirect.RouteValues["questionNameUrl"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };

        var questionDetails = new QuestionDetails
        {
            QuestionId = questionId,
            CurrentQuestionUrl = "task/question",
            QuestionTypeName = "fileUpload",
            QuestionContent = "{}"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(questionDetails);

        _attachmentServiceMock.Setup(a =>
            a.DeleteLinkedFile(LinkType.Question, questionId, attachmentId, application.ApplicationId))
            .ReturnsAsync(true);

        _controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/json";

        // Act
        var result = await _controller.DeleteFile("task", "question", attachmentId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("File deleted successfully.", okResult.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DownloadFile_ReturnsUnauthorised_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.DownloadFile("task", "question", Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DownloadFile_ReturnsNotFound_WhenQuestionIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.DownloadFile("task", "question", Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DownloadFile_ReturnsNotFound_WhenAttachmentNotInList()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = questionId,
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "File",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        _attachmentServiceMock.Setup(x =>
            x.GetAllLinkedFiles(LinkType.Question, questionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>()); // No matching attachment

        // Act
        var result = await _controller.DownloadFile("task", "question", Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DownloadFile_ReturnsBadRequest_WhenStreamIsNull()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };

        var attachment = new AttachmentDetails
        {
            AttachmentId = attachmentId,
            FileName = "file.txt",
            FileMIMEtype = "text/plain",
            FileSize = 1000
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = questionId,
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "File",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        _attachmentServiceMock.Setup(x =>
            x.GetAllLinkedFiles(LinkType.Question, questionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails> { attachment });

        _attachmentServiceMock.Setup(x =>
            x.DownloadLinkedFile(LinkType.Question, questionId, attachmentId, application.ApplicationId))
            .ReturnsAsync((Stream?)null);

        // Act
        var result = await _controller.DownloadFile("task", "question", attachmentId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to retrieve file.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DownloadFile_ReturnsFileResult_WhenSuccessful()
    {
        // Arrange
        var questionId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };

        var attachment = new AttachmentDetails
        {
            AttachmentId = attachmentId,
            FileName = "example.txt",
            FileMIMEtype = "text/plain",
            FileSize = 2048
        };

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = questionId,
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "File",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        _attachmentServiceMock.Setup(x =>
            x.GetAllLinkedFiles(LinkType.Question, questionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails> { attachment });

        var stream = new MemoryStream(Encoding.UTF8.GetBytes("File content"));

        _attachmentServiceMock.Setup(x =>
            x.DownloadLinkedFile(LinkType.Question, questionId, attachmentId, application.ApplicationId))
            .ReturnsAsync(stream);

        // Act
        var result = await _controller.DownloadFile("task", "question", attachmentId);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("text/plain", fileResult.ContentType);
        Assert.Equal("example.txt", fileResult.FileDownloadName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ListFiles_ReturnsUnauthorised_WhenApplicationIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns((Application?)null);

        // Act
        var result = await _controller.ListFiles("task", "question");

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ListFiles_ReturnsNotFound_WhenQuestionDetailsAreNull()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.ListFiles("task", "question");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ListFiles_ReturnsOk_WithEmptyResponse_WhenNoAttachmentsExist()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var questionDetails = new QuestionDetails
        {
            QuestionId = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "File",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(questionDetails);

        _attachmentServiceMock
            .Setup(a => a.GetAllLinkedFiles(LinkType.Question, questionDetails.QuestionId, application.ApplicationId))
            .ReturnsAsync(new List<AttachmentDetails>());

        // Act
        var result = await _controller.ListFiles("task", "question");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
        Assert.Empty(response);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ListFiles_ReturnsOk_WithFileDetails_WhenAttachmentsExist()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var questionId = Guid.NewGuid();

        var questionDetails = new QuestionDetails
        {
            QuestionId = questionId,
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "File",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var attachments = new List<AttachmentDetails>
        {
            new AttachmentDetails
            {
                FileName = "file1.txt",
                FileSize = 12345,
                FileMIMEtype = "text/plain",
                AttachmentId = Guid.NewGuid()
            },
            new AttachmentDetails
            {
                FileName = "file2.pdf",
                FileSize = 54321,
                FileMIMEtype = "application/pdf",
                AttachmentId = Guid.NewGuid()
            }
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(questionDetails);

        _attachmentServiceMock.Setup(a =>
            a.GetAllLinkedFiles(LinkType.Question, questionId, application.ApplicationId))
            .ReturnsAsync(attachments);

        // Act
        var result = await _controller.ListFiles("task", "question");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var responseList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(
            JsonConvert.SerializeObject(okResult.Value)
        );

        Assert.Equal(2, responseList!.Count);

        foreach (var item in responseList)
        {
            Assert.True(item.ContainsKey("fieldName"));
            Assert.True(item.ContainsKey("length"));
            Assert.True(item.ContainsKey("attachmentId"));
        }
    }
}
