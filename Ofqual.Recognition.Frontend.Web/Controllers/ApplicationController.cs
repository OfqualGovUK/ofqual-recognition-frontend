using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using System.Threading.Tasks;

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

    public ApplicationController(
        IApplicationService applicationService, 
        ITaskService taskService, 
        ISessionService sessionService, 
        IQuestionService questionService, 
        IAttachmentService attachmentService)
    {
        _applicationService = applicationService;
        _taskService = taskService;
        _sessionService = sessionService;
        _questionService = questionService;
        _attachmentService = attachmentService;
    }

    [HttpGet]
    public async Task<IActionResult> StartApplication()
    {
        Application? application = await _applicationService.SetUpApplication();
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

        QuestionAnswer? questionAnswer = await _questionService.GetQuestionAnswer(application.ApplicationId, questionDetails.QuestionId);

        var status = _sessionService.GetTaskStatusFromSession(questionDetails.TaskId);
        if (status == TaskStatusEnum.Completed && !fromReview)
        {
            return RedirectToAction(nameof(TaskReview), new { taskNameUrl });
        }

        var linkedAttachments = await _attachmentService.GetAllLinkedFiles(LinkType.Question, questionDetails.QuestionId, application.ApplicationId);
 
        QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);
        questionViewModel.FromReview = fromReview;
        questionViewModel.AnswerJson = questionAnswer?.Answer;
        questionViewModel.Attachments = AttachmentMapper.MapToViewModel(linkedAttachments);

        return View(questionViewModel);
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuestionDetails(string taskNameUrl, string questionNameUrl, [FromForm] IFormCollection formdata)
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page instead of home
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        var questionDetails = await _questionService.GetQuestionDetails(taskNameUrl, questionNameUrl);
        if (questionDetails == null)
        {
            return NotFound();
        }

        var jsonAnswer = JsonHelper.ConvertToJson(formdata);
        var existingAnswer = await _questionService.GetQuestionAnswer(application.ApplicationId, questionDetails.QuestionId);

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

        //If there are no more questions, we need to check stage types and determine where to go
        if (string.IsNullOrEmpty(questionDetails.NextQuestionUrl))
        {
            var taskDetails = await _taskService.GetTaskDetailsByTaskNameUrl(taskNameUrl);

            //if this is the application review, we can skip task review and go to list page
            if (taskDetails != null 
                && taskDetails.TaskStages
                    .Intersect([TaskStage.Review, TaskStage.Information])
                    .Any())
                return Redirect(RouteConstants.HomeConstants.HOME_PATH);                

            //otherwise go to the task review page
            return RedirectToAction(nameof(TaskReview), new { taskNameUrl });
        }

        //more questions to do, go to the next in line
        var nextQuestion = QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl);
        if (!string.IsNullOrEmpty(questionDetails.NextQuestionUrl) && nextQuestion == null)
        {
            return BadRequest();
        }

        return RedirectToAction(nameof(QuestionDetails), new
        {
            nextQuestion!.Value.taskNameUrl,
            nextQuestion!.Value.questionNameUrl
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

        TaskStatusEnum? status = _sessionService.GetTaskStatusFromSession(taskDetails.TaskId);
        if (status == null)
        {
            return BadRequest();
        }

        var lastQuestionUrl = reviewAnswers.LastOrDefault()?
            .QuestionAnswers.LastOrDefault()?
            .QuestionUrl;        

        TaskReviewViewModel taskReview = QuestionMapper.MapToViewModel(reviewAnswers);
        taskReview.LastQuestionUrl = lastQuestionUrl;
        taskReview.IsCompletedStatus = status == TaskStatusEnum.Completed;
        taskReview.Answer = (TaskStatusEnum)status;
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

        if (formdata.Answer != TaskStatusEnum.Completed && formdata.Answer != TaskStatusEnum.InProgress)
        {
            return BadRequest();
        }

        bool hasTaskStatusUpdated = await _taskService.UpdateTaskStatus(application.ApplicationId, taskDetails.TaskId, formdata.Answer);
        if (!hasTaskStatusUpdated)
        {
            return BadRequest();
        }

        //If this is a declaration, we go through to complete the application
        if (taskDetails.TaskStages.Contains(TaskStage.Declare))
        {
            if (await _applicationService.CompleteApplication(application.ApplicationId))
                return RedirectToAction(nameof(ConfirmSubmission));
            return BadRequest("Failed to submit application for submission");
        }


        return Redirect(RouteConstants.ApplicationConstants.TASK_LIST_PATH);
    }

    [HttpGet("confirm-submission")]
    public IActionResult ConfirmSubmission()
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        if (application == null)
        {
            // TODO: Redirect to login page instead of home
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }
        return View();
    }
}