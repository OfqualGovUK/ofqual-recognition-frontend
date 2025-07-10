using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Authorize]
[AuthorizeForScopes(ScopeKeySection = "RecognitionApi:Scopes")]
[Route("application")]
public class ApplicationController : Controller
{
    private readonly IApplicationService _applicationService;
    private readonly ITaskService _taskService;
    private readonly ISessionService _sessionService;
    private readonly IQuestionService _questionService;
    private readonly IAttachmentService _attachmentService;
    private readonly IPreEngagementService _preEngagementService;

    public ApplicationController(
        IApplicationService applicationService,
        ITaskService taskService,
        ISessionService sessionService,
        IQuestionService questionService,
        IAttachmentService attachmentService,
        IPreEngagementService preEngagementService)
    {
        _applicationService = applicationService;
        _taskService = taskService;
        _sessionService = sessionService;
        _questionService = questionService;
        _attachmentService = attachmentService;
        _preEngagementService = preEngagementService;
    }

    [HttpGet]
    public async Task<IActionResult> InitialiseApplication()
    {
        Application? application = await _applicationService.InitialiseApplication();
        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        return RedirectToAction(nameof(TaskList));
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> TaskList()
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        var taskItemStatusSection = await _taskService.GetApplicationTasks(application.ApplicationId);

        TaskListViewModel taskListViewModel = TaskListMapper.MapToViewModel(taskItemStatusSection);

        return View(taskListViewModel);
    }

    [HttpGet("{taskNameUrl}/{questionNameUrl}")]
    public async Task<IActionResult> QuestionDetails(string taskNameUrl, string questionNameUrl, bool fromReview = false)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        StatusType? status = _sessionService.GetTaskStatusFromSession(questionDetails.TaskId);

        // Check if the task is in progress or completed and if the question is not an application review
        if (status == StatusType.Completed && !fromReview && questionDetails.QuestionTypeName != QuestionType.Review)
        {
            return RedirectToAction(nameof(TaskReview), new { taskNameUrl });
        }

        if (status == StatusType.InProgress && questionDetails.QuestionTypeName == QuestionType.PreEngagement && !string.IsNullOrEmpty(questionDetails.NextQuestionUrl))
        {
            return RedirectToAction(nameof(QuestionDetails), new
            {
                QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl)!.Value.taskNameUrl,
                QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl)!.Value.questionNameUrl
            });
        }

        QuestionAnswer? questionAnswer = await _questionService.GetQuestionAnswer(application.ApplicationId, questionDetails.QuestionId);

        var linkedAttachments = new List<AttachmentDetails>();
        var applicationReviewAnswers = new List<TaskReviewSection>();

        if (questionDetails.QuestionTypeName == QuestionType.FileUpload)
        {
            linkedAttachments = await _attachmentService.GetAllLinkedFiles(LinkType.Question, questionDetails.QuestionId, application.ApplicationId);
        }

        if (questionDetails.QuestionTypeName == QuestionType.Review)
        {
            applicationReviewAnswers = await _questionService.GetAllApplicationAnswers(application.ApplicationId);
        }

        QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);
        questionViewModel.FromReview = fromReview;
        questionViewModel.AnswerJson = questionAnswer?.Answer;
        questionViewModel.Attachments = AttachmentMapper.MapToViewModel(linkedAttachments);
        questionViewModel.TaskReviewSection = ApplicationAnswersMapper.MapToViewModel(applicationReviewAnswers);

        return View(questionViewModel);
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuestionDetails(string taskNameUrl, string questionNameUrl, [FromForm] IFormCollection formdata)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page instead of home
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        string jsonAnswer = JsonHelper.ConvertToJson(formdata);
        QuestionAnswer? existingAnswer = await _questionService.GetQuestionAnswer(application.ApplicationId, questionDetails.QuestionId);

        if (!JsonHelper.AreEqual(existingAnswer?.Answer, jsonAnswer))
        {
            ValidationResponse? validationResponse = await _questionService.SubmitQuestionAnswer(application.ApplicationId, questionDetails.TaskId, questionDetails.QuestionId, jsonAnswer);
            if (validationResponse == null)
            {
                return BadRequest();
            }

            if (validationResponse.Errors != null && validationResponse.Errors.Any())
            {
                QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);
                questionViewModel.Validation = QuestionMapper.MapToViewModel(validationResponse);
                questionViewModel.AnswerJson = jsonAnswer;

                return View(questionViewModel);
            }
        }

        // If the question type is review, update the task status to completed and redirect to the task list
        if (questionDetails.QuestionTypeName == QuestionType.Review)
        {
            var updateStatusSucceeded = await _taskService.UpdateTaskStatus(application.ApplicationId, questionDetails.TaskId, StatusType.Completed);
            if (!updateStatusSucceeded)
            {
                return BadRequest();
            }
            _sessionService.UpdateTaskStatusInSession(questionDetails.TaskId, StatusType.Completed);
            return Redirect(RouteConstants.ApplicationConstants.TASK_LIST_PATH);
        }

        if (string.IsNullOrEmpty(questionDetails.NextQuestionUrl))
        {
            return RedirectToAction(nameof(TaskReview), new { taskNameUrl });
        }

        return RedirectToAction(nameof(QuestionDetails), new
        {
            QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl)!.Value.taskNameUrl,
            QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl)!.Value.questionNameUrl
        });
    }

    [HttpGet("{taskNameUrl}/review-your-answers")]
    public async Task<IActionResult> TaskReview(string taskNameUrl)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        TaskDetails? taskDetails = await _taskService.GetTaskDetailsByTaskNameUrl(taskNameUrl);
        if (taskDetails == null)
        {
            return NotFound();
        }

        var reviewAnswers = await _questionService.GetTaskQuestionAnswers(application.ApplicationId, taskDetails.TaskId);
        if (reviewAnswers == null || reviewAnswers.Count == 0)
        {
            return NotFound();
        }

        StatusType? status = _sessionService.GetTaskStatusFromSession(taskDetails.TaskId);
        if (status == null)
        {
            return BadRequest();
        }

        var lastQuestionUrl = reviewAnswers.LastOrDefault()?
            .QuestionAnswers.LastOrDefault()?
            .QuestionUrl;

        TaskReviewViewModel taskReview = QuestionMapper.MapToViewModel(reviewAnswers);
        taskReview.LastQuestionUrl = lastQuestionUrl;
        taskReview.IsCompletedStatus = status == StatusType.Completed;
        taskReview.Answer = (StatusType)status;
        return View(taskReview);
    }

    [HttpPost("{taskNameUrl}/review-your-answers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaskReview(string taskNameUrl, [FromForm] TaskReviewViewModel formdata)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        TaskDetails? taskDetails = await _taskService.GetTaskDetailsByTaskNameUrl(taskNameUrl);
        if (taskDetails == null)
        {
            return NotFound();
        }

        if (formdata.Answer != StatusType.Completed && formdata.Answer != StatusType.InProgress)
        {
            return BadRequest();
        }

        bool updateSucceeded = await _taskService.UpdateTaskStatus(application.ApplicationId, taskDetails.TaskId, formdata.Answer);
        if (!updateSucceeded)
        {
            return BadRequest();
        }

        if (taskDetails.Stage == StageType.Declaration)
        {
            Application? submitted = await _applicationService.SubmitApplication(application.ApplicationId);
            if (submitted == null || !submitted.Submitted)
            {
                return BadRequest("Could not submit application.");
            }

            return RedirectToAction(nameof(ConfirmSubmission));
        }

        return Redirect(RouteConstants.ApplicationConstants.TASK_LIST_PATH);
    }

    [HttpPost("{taskNameUrl}/request-information")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestInformation(string taskNameUrl)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page instead of home
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        TaskDetails? taskDetails = await _taskService.GetTaskDetailsByTaskNameUrl(taskNameUrl);
        if (taskDetails == null)
        {
            return NotFound();
        }

        if (taskDetails.Stage != StageType.MainApplication)
        {
            return BadRequest();
        }

        bool success = await _preEngagementService.SendPreEngagementInformationEmail(application.ApplicationId);
        if (!success)
        {
            return BadRequest("Failed to process request information.");
        }

        bool updateSucceeded = await _taskService.UpdateTaskStatus(application.ApplicationId, taskDetails.TaskId, StatusType.InProgress);
        if (!updateSucceeded)
        {
            return BadRequest();
        }

        return RedirectToAction(nameof(PreEngagementController.PreEngagementConfirmation), "PreEngagement");
    }

    [HttpGet("confirm-submission")]
    public IActionResult ConfirmSubmission()
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page instead of home
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        if (!application.Submitted)
        {
            return BadRequest();
        }

        return View();
    }
}