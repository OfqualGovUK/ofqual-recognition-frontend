using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Web.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Authorize]
[AuthorizeForScopes(ScopeKeySection = "RecognitionApi:Scopes")]
[Route("application")]
public class FileUploadController : Controller
{
    private readonly ISessionService _sessionService;
    private readonly IQuestionService _questionService;
    private readonly IAttachmentService _attachmentService;
    private readonly ITaskService _taskService;

    public FileUploadController(ISessionService sessionService, IQuestionService questionService, IAttachmentService attachmentService, ITaskService taskService)
    {
        _sessionService = sessionService;
        _questionService = questionService;
        _attachmentService = attachmentService;
        _taskService = taskService;
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}/submit")]
    public async Task<IActionResult> Submit(string taskNameUrl, string questionNameUrl, [FromForm] List<IFormFile>? files)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            return Unauthorized();
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        var sessionId = HttpContext.Session.Id;
        var questionId = questionDetails.QuestionId;

        if (files != null && files.Any())
        {
            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                if (file.Length > 25 * 1024 * 1024)
                {
                    continue;
                }

                if (!FileValidationHelper.IsAllowedFile(file))
                {
                    continue;
                }

                if (AttachmentStore.IsDuplicate(sessionId, questionId, file.FileName, file.Length))
                {
                    continue;
                }

                var attachment = await _attachmentService.UploadFileToLinkedRecord(LinkType.Question, questionId, application.ApplicationId, file);
                if (attachment != null)
                {
                    var fileId = Guid.NewGuid();
                    AttachmentStore.TryAdd(sessionId, questionId, fileId, attachment);
                }
            }
        }

        bool hasTaskStatusUpdated = await _taskService.UpdateTaskStatus(application.ApplicationId, questionDetails.TaskId, TaskStatusEnum.InProgress);
        if (!hasTaskStatusUpdated)
        {
            return BadRequest();
        }

        if (string.IsNullOrEmpty(questionDetails.NextQuestionUrl))
        {
            return RedirectToAction(nameof(ApplicationController.TaskReview), "Application", new { taskNameUrl });

        }

        var nextQuestion = QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl);
        if (!string.IsNullOrEmpty(questionDetails.NextQuestionUrl) && nextQuestion == null)
        {
            return BadRequest();
        }

        return RedirectToAction(nameof(ApplicationController.QuestionDetails), "Application", new
        {
            nextQuestion!.Value.taskNameUrl,
            nextQuestion.Value.questionNameUrl
        });
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}/upload")]
    public async Task<IActionResult> Upload(string taskNameUrl, string questionNameUrl, IFormFile file, [FromForm] Guid fileId)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            return Unauthorized();
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("The selected file is empty.");
        }

        if (file.Length > 25 * 1024 * 1024)
        {
            return BadRequest("The selected file must be smaller than 25MB.");
        }

        if (!FileValidationHelper.IsAllowedFile(file))
        {
            return BadRequest("Unsupported file type or content.");
        }

        var sessionId = HttpContext.Session.Id;
        var questionId = questionDetails.QuestionId;

        if (AttachmentStore.IsDuplicate(sessionId, questionId, file.FileName, file.Length))
        {
            return BadRequest("This file has already been uploaded.");
        }

        AttachmentDetails? attachmentDetails = await _attachmentService.UploadFileToLinkedRecord(LinkType.Question, questionId, application.ApplicationId, file);
        if (attachmentDetails == null)
        {
            return BadRequest("Failed to upload file.");
        }

        bool isAdded = AttachmentStore.TryAdd(sessionId, questionId, fileId, attachmentDetails);
        if (!isAdded)
        {
            return Conflict("This file has already been uploaded.");
        }

        bool hasTaskStatusUpdated = await _taskService.UpdateTaskStatus(application.ApplicationId, questionDetails.TaskId, TaskStatusEnum.InProgress);
        if (!hasTaskStatusUpdated)
        {
            return BadRequest();
        }

        return Ok("File uploaded successfully.");
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}/delete")]
    public async Task<IActionResult> Delete(string taskNameUrl, string questionNameUrl, [FromForm] Guid fileId)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            return Unauthorized();
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        var sessionId = HttpContext.Session.Id;
        var questionId = questionDetails.QuestionId;

        if (!AttachmentStore.TryRemove(sessionId, questionId, fileId, out var attachment) || attachment == null)
        {
            return NotFound("File not found.");
        }

        bool isDeleted = await _attachmentService.DeleteLinkedFile(LinkType.Question, questionId, attachment.AttachmentId, application.ApplicationId);
        if (!isDeleted)
        {
            return BadRequest("Failed to delete file.");
        }

        return Ok("File deleted successfully.");
    }

    [HttpGet("{taskNameUrl}/{questionNameUrl}/download/{fileId}")]
    public async Task<IActionResult> Download(string taskNameUrl, string questionNameUrl, Guid fileId)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            return Unauthorized();
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        var sessionId = HttpContext.Session.Id;
        var questionId = questionDetails.QuestionId;

        if (!AttachmentStore.TryRemove(sessionId, questionId, fileId, out var attachment) || attachment == null)
        {
            return NotFound("File not found.");
        }

        Stream? stream = await _attachmentService.DownloadLinkedFile(LinkType.Question, questionId, attachment.AttachmentId, application.ApplicationId);
        if (stream == null)
        {
            return BadRequest("Failed to retrieve file.");
        }

        return File(stream, attachment.FileMIMEtype, attachment.FileName);
    }

    [HttpGet("{taskNameUrl}/{questionNameUrl}/list")]
    public async Task<IActionResult> List(string taskNameUrl, string questionNameUrl)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            return Unauthorized();
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        var attachments = await _attachmentService.GetAllLinkedFiles(
            LinkType.Question,
            questionDetails.QuestionId,
            application.ApplicationId
        );

        var sessionId = HttpContext.Session.Id;
        var questionId = questionDetails.QuestionId;

        AttachmentStore.Clear(sessionId, questionId);

        var response = new Dictionary<Guid, object>();
        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                var fileId = Guid.NewGuid();
                AttachmentStore.TryAdd(sessionId, questionId, fileId, attachment);

                response[fileId] = new
                {
                    fileName = attachment.FileName,
                    length = attachment.FileSize
                };
            }
        }

        return Ok(response);
    }
}