using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("application")]
public class ApplicationController : Controller
{
    private readonly IApplicationService _applicationService;
    private readonly ITaskService _taskService;
    private readonly ISessionService _sessionService;
    private readonly IQuestionService _questionService;

    public ApplicationController(IApplicationService applicationService, ITaskService taskService, ISessionService sessionService, IQuestionService questionService)
    {
        _applicationService = applicationService;
        _taskService = taskService;
        _sessionService = sessionService;
        _questionService = questionService;
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

        var status = _sessionService.GetTaskStatusFromSession(questionDetails.TaskId);

        if (status == TaskStatusEnum.Completed && !fromReview)
        {
            return RedirectToAction(nameof(TaskReview), new
            {
                taskNameUrl,
                questionNameUrl
            });
        }

        QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);
        questionViewModel.FromReview = fromReview;

        return View(questionViewModel);
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitAnswers(string taskNameUrl, string questionNameUrl, [FromForm] IFormCollection formdata)
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

        // Temporary solution might need to be refactored in the future
        string jsonAnswer = FormDataHelper.ConvertToJson(formdata);

        QuestionAnswerSubmissionResponse? questionAnswerResult = await _questionService.SubmitQuestionAnswer(
            application.ApplicationId,
            questionDetails.TaskId,
            questionDetails.QuestionId,
            jsonAnswer
        );

        if (questionAnswerResult == null)
        {
            return RedirectToAction(nameof(TaskReview), new
            {
                taskNameUrl
            });
        }

        return RedirectToAction(nameof(QuestionDetails), new
        {
            taskNameUrl = questionAnswerResult.NextTaskNameUrl,
            questionNameUrl = questionAnswerResult.NextQuestionNameUrl
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

        TaskItem? taskDetails = await _taskService.GetTaskDetailsByTaskNameUrl(taskNameUrl);

        if (taskDetails == null)
        {
            return NotFound();
        }

        var reviewAnswers = await _questionService.GetTaskQuestionAnswers(application.ApplicationId, taskDetails.TaskId);

        if (reviewAnswers == null || reviewAnswers.Count == 0)
        {
            return NotFound();
        }

        var lastQuestionUrl = reviewAnswers.LastOrDefault()?
            .QuestionAnswers.LastOrDefault()?
            .QuestionUrl;
        var status = _sessionService.GetTaskStatusFromSession(taskDetails.TaskId);

        TaskReviewViewModel taskReview = QuestionMapper.MapToViewModel(reviewAnswers);
        taskReview.LastQuestionUrl = lastQuestionUrl;
        taskReview.IsCompletedStatus = status == TaskStatusEnum.Completed;

        return View(taskReview);
    }

    [HttpPost("{taskNameUrl}/review-your-answers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitTaskReview(string taskNameUrl, [FromForm] TaskReviewViewModel formdata)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        TaskItem? taskDetails = await _taskService.GetTaskDetailsByTaskNameUrl(taskNameUrl);

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

        return Redirect(RouteConstants.ApplicationConstants.TASK_LIST_PATH);
    }
}