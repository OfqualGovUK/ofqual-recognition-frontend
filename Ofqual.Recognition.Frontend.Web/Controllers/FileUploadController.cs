using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
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

    [HttpPost("{taskNameUrl}/{questionNameUrl}/file-submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitFile(string taskNameUrl, string questionNameUrl, [FromForm] List<IFormFile>? files)
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

        QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);

        var existingAttachments = await _attachmentService.GetAllLinkedFiles(LinkType.Question, questionDetails.QuestionId, application.ApplicationId);

        var allAttachments = new List<AttachmentDetails>(existingAttachments);
        var validationErrors = new List<ErrorItemViewModel>();

        if (files != null && files.Any())
        {
            foreach (var file in files)
            {
                var fieldName = questionViewModel?.QuestionContent?.FormGroup?.FileUpload?.Name ?? "files";

                if (file == null || file.Length == 0)
                {
                    validationErrors.Add(new ErrorItemViewModel
                    {
                        PropertyName = fieldName,
                        ErrorMessage = $"The file \"{file?.FileName}\" is empty."
                    });
                    continue;
                }

                if (file.Length > 25 * 1024 * 1024)
                {
                    validationErrors.Add(new ErrorItemViewModel
                    {
                        PropertyName = fieldName,
                        ErrorMessage = $"The file \"{file.FileName}\" must be smaller than 25MB."
                    });
                    continue;
                }

                if (!FileValidationHelper.IsAllowedFile(file))
                {
                    validationErrors.Add(new ErrorItemViewModel
                    {
                        PropertyName = fieldName,
                        ErrorMessage = $"The file \"{file.FileName}\" has an unsupported type or content."
                    });
                    continue;
                }

                var attachments = await _attachmentService.GetAllLinkedFiles(LinkType.Question, questionDetails.QuestionId, application.ApplicationId);

                bool isDuplicate = allAttachments.Any(a => a.FileName.Equals(file.FileName, StringComparison.OrdinalIgnoreCase) && a.FileSize == file.Length);
                if (isDuplicate)
                {
                    validationErrors.Add(new ErrorItemViewModel
                    {
                        PropertyName = fieldName,
                        ErrorMessage = $"The file \"{file.FileName}\" has already been uploaded."
                    });
                    continue;
                }

                var attachment = await _attachmentService.UploadFileToLinkedRecord(LinkType.Question, questionDetails.QuestionId, application.ApplicationId, file);
                if (attachment != null)
                {
                    allAttachments.Add(attachment);
                }
                else
                {
                    validationErrors.Add(new ErrorItemViewModel
                    {
                        PropertyName = fieldName,
                        ErrorMessage = $"Failed to upload the file \"{file.FileName}\"."
                    });
                }
            }
        }

        if (questionViewModel?.QuestionContent?.FormGroup?.FileUpload?.Validation?.Required == true && !validationErrors.Any())
        {
            validationErrors.Add(new ErrorItemViewModel
            {
                PropertyName = questionViewModel.QuestionContent.FormGroup.FileUpload.Name ?? "files",
                ErrorMessage = "You must upload a file to continue."
            });
        }

        if (validationErrors.Any())
        {
            questionViewModel!.Attachments = AttachmentMapper.MapToViewModel(allAttachments);
            questionViewModel!.Validation = new ValidationViewModel
            {
                Errors = validationErrors
            };

            return View("~/Views/Application/QuestionDetails.cshtml", questionViewModel);
        }

        bool hasTaskStatusUpdated = await _taskService.UpdateTaskStatus(application.ApplicationId, questionDetails.TaskId, TaskStatusEnum.InProgress);
        if (!hasTaskStatusUpdated)
        {
            return BadRequest();
        }

        if (string.IsNullOrEmpty(questionDetails.NextQuestionUrl))
        {
            _sessionService.ClearFromSession($"{SessionKeys.ApplicationQuestionReview}:{application.ApplicationId}:{questionDetails.TaskId}");
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
            nextQuestion!.Value.questionNameUrl
        });
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}/upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadFile(string taskNameUrl, string questionNameUrl, IFormFile file)
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

        var attachments = await _attachmentService.GetAllLinkedFiles(LinkType.Question, questionDetails.QuestionId, application.ApplicationId);

        bool isDuplicate = attachments.Any(a => a.FileName.Equals(file.FileName, StringComparison.OrdinalIgnoreCase) && a.FileSize == file.Length);
        if (isDuplicate)
        {
            return BadRequest("This file has already been uploaded.");
        }

        AttachmentDetails? attachmentDetails = await _attachmentService.UploadFileToLinkedRecord(LinkType.Question, questionDetails.QuestionId, application.ApplicationId, file);
        if (attachmentDetails == null)
        {
            return BadRequest("Failed to upload file.");
        }

        bool hasTaskStatusUpdated = await _taskService.UpdateTaskStatus(application.ApplicationId, questionDetails.TaskId, TaskStatusEnum.InProgress);
        if (!hasTaskStatusUpdated)
        {
            return BadRequest();
        }

        _sessionService.ClearFromSession($"{SessionKeys.ApplicationQuestionReview}:{application.ApplicationId}:{questionDetails.TaskId}");
        return Ok(attachmentDetails.AttachmentId);
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}/delete/{attachmentId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFile(string taskNameUrl, string questionNameUrl, Guid attachmentId)
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

        bool isDeleted = await _attachmentService.DeleteLinkedFile(LinkType.Question, questionDetails.QuestionId, attachmentId, application.ApplicationId);
        if (!isDeleted)
        {
            return BadRequest("Failed to delete file.");
        }

        bool isHtmlRequest = Request.Headers["Accept"].Any(h => h != null && h.Contains("text/html", StringComparison.OrdinalIgnoreCase));
        if (isHtmlRequest)
        {
            var currentQuestion = QuestionUrlHelper.Parse(questionDetails.CurrentQuestionUrl);
            if (!string.IsNullOrEmpty(questionDetails.NextQuestionUrl) && currentQuestion == null)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(ApplicationController.QuestionDetails), "Application", new
            {
                currentQuestion!.Value.taskNameUrl,
                currentQuestion!.Value.questionNameUrl
            });
        }

        return Ok("File deleted successfully.");
    }

    [HttpGet("{taskNameUrl}/{questionNameUrl}/download/{attachmentId}")]
    public async Task<IActionResult> DownloadFile(string taskNameUrl, string questionNameUrl, Guid attachmentId)
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            return Unauthorized();
        }

        var questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        var attachments = await _attachmentService.GetAllLinkedFiles(
            LinkType.Question,
            questionDetails.QuestionId,
            application.ApplicationId
        );

        var attachment = attachments.FirstOrDefault(a => a.AttachmentId == attachmentId);
        if (attachment == null)
        {
            return NotFound();
        }

        var stream = await _attachmentService.DownloadLinkedFile(LinkType.Question, questionDetails.QuestionId, attachmentId, application.ApplicationId);
        if (stream == null)
        {
            return BadRequest("Failed to retrieve file.");
        }

        return File(stream, attachment.FileMIMEtype, attachment.FileName);
    }

    [HttpGet("{taskNameUrl}/{questionNameUrl}/list")]
    public async Task<IActionResult> ListFiles(string taskNameUrl, string questionNameUrl)
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            return Unauthorized();
        }

        var questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        var attachments = await _attachmentService.GetAllLinkedFiles(
            LinkType.Question,
            questionDetails.QuestionId,
            application.ApplicationId
        );

        var response = attachments.Select(a => new
        {
            fieldName = a.FileName,
            length = a.FileSize,
            attachmentId = a.AttachmentId
        });

        return Ok(response);
    }
}