using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Controllers;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Tests.Helpers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ofqual.Recognition.Frontend.Web.Stores;
using System.Text;

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
                HttpContext = new DefaultHttpContext
                {
                    Session = new FakeSession("test-session")
                }
            }
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitFile_Returns_Unauthorised_When_Application_Is_Null()
    {
        // Arrange
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns((Application?)null);

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
        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(new Application());
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync((QuestionDetails?)null);

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

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);

        // Act
        var result = await _controller.SubmitFile("task", "question", new List<IFormFile> { fileMock.Object });

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
        Assert.NotEmpty(model.Validation!.Errors!);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitFile_Redirects_To_Review_When_No_More_Questions()
    {
        // Arrange
        var application = new Application { ApplicationId = Guid.NewGuid() };
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

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _attachmentServiceMock.Setup(x => x.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file)).ReturnsAsync(new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "file.txt",
            FileMIMEtype = "text/plain",
            FileSize = 100
        });
        _taskServiceMock.Setup(x => x.UpdateTaskStatus(application.ApplicationId, question.TaskId, TaskStatusEnum.InProgress)).ReturnsAsync(true);

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
        var application = new Application { ApplicationId = Guid.NewGuid() };
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

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _attachmentServiceMock.Setup(x => x.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file)).ReturnsAsync(new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "file.txt",
            FileMIMEtype = "text/plain",
            FileSize = 100
        });
        _taskServiceMock.Setup(x => x.UpdateTaskStatus(application.ApplicationId, question.TaskId, TaskStatusEnum.InProgress)).ReturnsAsync(false);

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
        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns((Application?)null);

        // Act
        var result = await _controller.UploadFile("task", "question", null!, Guid.NewGuid());

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task UploadFile_ReturnsNotFound_WhenQuestionDetailsIsNull()
    {
        // Arrange
        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(new Application());
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync((QuestionDetails?)null);

        // Act
        var result = await _controller.UploadFile("task", "question", null!, Guid.NewGuid());

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
        var result = await _controller.UploadFile("task", "question", file, Guid.NewGuid());

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
        var result = await _controller.UploadFile("task", "question", file, Guid.NewGuid());

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
        var result = await _controller.UploadFile("task", "question", file, Guid.NewGuid());

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

        AttachmentStore.TryAdd("test-session", questionId, Guid.NewGuid(), new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "file.txt",
            FileMIMEtype = "text/plain",
            FileSize = 100
        });

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync(question);

        // Act
        var result = await _controller.UploadFile("task", "question", file, Guid.NewGuid());

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
        _attachmentServiceMock.Setup(a => a.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file))
                              .ReturnsAsync((AttachmentDetails?)null);

        // Act
        var result = await _controller.UploadFile("task", "question", file, Guid.NewGuid());

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

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _attachmentServiceMock.Setup(a => a.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file))
                              .ReturnsAsync(attachment);
        _taskServiceMock.Setup(t => t.UpdateTaskStatus(application.ApplicationId, question.TaskId, TaskStatusEnum.InProgress))
                        .ReturnsAsync(false);

        // Act
        var result = await _controller.UploadFile("task", "question", file, Guid.NewGuid());

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

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application)).Returns(application);
        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question")).ReturnsAsync(question);
        _attachmentServiceMock.Setup(a => a.UploadFileToLinkedRecord(LinkType.Question, question.QuestionId, application.ApplicationId, file))
                              .ReturnsAsync(attachment);
        _taskServiceMock.Setup(t => t.UpdateTaskStatus(application.ApplicationId, question.TaskId, TaskStatusEnum.InProgress))
                        .ReturnsAsync(true);

        // Act
        var result = await _controller.UploadFile("task", "question", file, Guid.NewGuid());

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("File uploaded successfully.", ok.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_ReturnsUnauthorized_WhenApplicationIsNull()
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
    public async Task DeleteFile_ReturnsNotFound_WhenAttachmentNotFound()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = questionId,
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "File",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        AttachmentStore.Clear("test-session", questionId); // Ensure file is not found

        // Act
        var result = await _controller.DeleteFile("task", "question", Guid.NewGuid());

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("File not found.", notFound.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_ReturnsBadRequest_WhenDeletionFails()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var attachment = new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "test.txt",
            FileSize = 1000,
            FileMIMEtype = "text/plain"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = questionId,
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "File",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        AttachmentStore.Clear("test-session", questionId);
        AttachmentStore.TryAdd("test-session", questionId, fileId, attachment);

        _attachmentServiceMock.Setup(a =>
            a.DeleteLinkedFile(LinkType.Question, questionId, attachment.AttachmentId, application.ApplicationId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteFile("task", "question", fileId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete file.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_ReturnsHtmlView_WhenHtmlRequest()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var questionDetails = new QuestionDetails
        {
            QuestionId = questionId,
            TaskId = Guid.NewGuid(),
            QuestionTypeName = "File",
            QuestionContent = "{}",
            CurrentQuestionUrl = "task/question"
        };

        var attachment = new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "test.txt",
            FileSize = 1000,
            FileMIMEtype = "text/plain"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(questionDetails);

        AttachmentStore.Clear("test-session", questionId);
        AttachmentStore.TryAdd("test-session", questionId, fileId, attachment);

        _attachmentServiceMock.Setup(a =>
            a.DeleteLinkedFile(LinkType.Question, questionId, attachment.AttachmentId, application.ApplicationId))
            .ReturnsAsync(true);

        _controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "text/html";

        // Act
        var result = await _controller.DeleteFile("task", "question", fileId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Application/QuestionDetails.cshtml", viewResult.ViewName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteFile_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var application = new Application { ApplicationId = Guid.NewGuid() };
        var attachment = new AttachmentDetails
        {
            AttachmentId = Guid.NewGuid(),
            FileName = "test.txt",
            FileSize = 1000,
            FileMIMEtype = "text/plain"
        };

        _sessionServiceMock.Setup(s => s.GetFromSession<Application>(SessionKeys.Application))
            .Returns(application);

        _questionServiceMock.Setup(q => q.GetQuestionDetails("task", "question"))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = questionId,
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "File",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        AttachmentStore.Clear("test-session", questionId);
        AttachmentStore.TryAdd("test-session", questionId, fileId, attachment);

        _attachmentServiceMock.Setup(a =>
            a.DeleteLinkedFile(LinkType.Question, questionId, attachment.AttachmentId, application.ApplicationId))
            .ReturnsAsync(true);

        _controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/json";

        // Act
        var result = await _controller.DeleteFile("task", "question", fileId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("File deleted successfully.", okResult.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DownloadFile_ReturnsUnauthorized_WhenApplicationIsNull()
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
    public async Task DownloadFile_ReturnsNotFound_WhenAttachmentNotFoundInStore()
    {
        // Arrange
        var questionId = Guid.NewGuid();

        _sessionServiceMock.Setup(x => x.GetFromSession<Application>(SessionKeys.Application))
            .Returns(new Application());

        _questionServiceMock.Setup(x => x.GetQuestionDetails("task", "question"))
            .ReturnsAsync(new QuestionDetails
            {
                QuestionId = questionId,
                TaskId = Guid.NewGuid(),
                QuestionTypeName = "File",
                QuestionContent = "{}",
                CurrentQuestionUrl = "task/question"
            });

        AttachmentStore.Clear("test-session", questionId);

        // Act
        var result = await _controller.DownloadFile("task", "question", Guid.NewGuid());

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("File not found.", notFound.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DownloadFile_ReturnsBadRequest_WhenStreamIsNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();

        var application = new Application { ApplicationId = applicationId };

        var attachment = new AttachmentDetails
        {
            FileName = "test.txt",
            FileSize = 1234,
            FileMIMEtype = "text/plain",
            AttachmentId = attachmentId
        };

        AttachmentStore.Clear("test-session", questionId);
        AttachmentStore.TryAdd("test-session", questionId, fileId, attachment);

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
            x.DownloadLinkedFile(LinkType.Question, questionId, attachmentId, applicationId))
            .ReturnsAsync((Stream?)null);

        // Act
        var result = await _controller.DownloadFile("task", "question", fileId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to retrieve file.", badRequest.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DownloadFile_ReturnsFileResult_WhenSuccessful()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();

        var attachment = new AttachmentDetails
        {
            FileName = "example.txt",
            FileSize = 2048,
            FileMIMEtype = "text/plain",
            AttachmentId = attachmentId
        };

        var application = new Application { ApplicationId = applicationId };

        AttachmentStore.Clear("test-session", questionId);
        AttachmentStore.TryAdd("test-session", questionId, fileId, attachment);

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

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello file content"));

        _attachmentServiceMock.Setup(x =>
            x.DownloadLinkedFile(LinkType.Question, questionId, attachmentId, applicationId))
            .ReturnsAsync(memoryStream);

        // Act
        var result = await _controller.DownloadFile("task", "question", fileId);

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("text/plain", fileResult.ContentType);
        Assert.Equal("example.txt", fileResult.FileDownloadName);
    }


    [Fact]
    [Trait("Category", "Unit")]
    public async Task ListFiles_ReturnsUnauthorized_WhenApplicationIsNull()
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
            .ReturnsAsync((List<AttachmentDetails>?)null);


        // Act
        var result = await _controller.ListFiles("task", "question");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dictionary = Assert.IsAssignableFrom<Dictionary<Guid, object>>(okResult.Value);
        Assert.Empty(dictionary);
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
        var response = Assert.IsAssignableFrom<Dictionary<Guid, object>>(okResult.Value);
        Assert.Equal(2, response.Count);

        foreach (var value in response.Values)
        {
            var file = Assert.IsAssignableFrom<Dictionary<string, object>>(value);
            Assert.True(file.ContainsKey("fileName"));
            Assert.True(file.ContainsKey("length"));
        }
    }
}
