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

    [HttpGet("{taskName}/{questionName}")]
    public async Task<IActionResult> QuestionDetails(string taskName, string questionName, bool fromReview = false)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskName, questionName);
        
        if (questionDetails == null)
        {
            return NotFound();
        }

        var status = _sessionService.GetTaskStatusFromSession(questionDetails.TaskId);

        if (status == TaskStatusEnum.Completed && !fromReview)
        {
            return RedirectToAction(nameof(TaskReview), new
            {
                taskName,
                questionName
            });
        }

        QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);
        questionViewModel.FromReview = fromReview;

        return View(questionViewModel);
    }

    [HttpPost("{taskName}/{questionName}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitAnswers(string taskName, string questionName, [FromForm] IFormCollection formdata)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);
        
        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskName, questionName);

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

        if (questionAnswerResult == null || questionAnswerResult.NextQuestionUrl == null)
        {
            return RedirectToAction(nameof(TaskReview), new
            {
                taskName,
                questionName
            });
        }

        var parsedUrl = QuestionUrlHelper.Parse(questionAnswerResult.NextQuestionUrl);

        if (parsedUrl == null)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(QuestionDetails), new
        {
            parsedUrl.Value.taskName,
            parsedUrl.Value.questionName
        });
    }

    [HttpGet("{taskName}/{questionName}/review-your-task-answers")]
    public async Task<IActionResult> TaskReview(string taskName, string questionName)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskName, questionName);

        if (questionDetails == null)
        {
            return NotFound();
        }

        var reviewAnswers = await _questionService.GetTaskQuestionAnswers(application.ApplicationId, questionDetails.TaskId);

        if (reviewAnswers == null || reviewAnswers.Count == 0)
        {
            return NotFound();
        }

        TaskReviewViewModel taskReview = QuestionMapper.MapToViewModel(reviewAnswers);

        return View(taskReview);
    }

    [HttpPost("{taskName}/{questionName}/review-your-task-answers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitTaskReview(string taskName, string questionName, [FromForm] TaskReviewViewModel formdata)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return Redirect(RouteConstants.HomeConstants.HOME_PATH);
        }

        QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(taskName, questionName);

        if (questionDetails == null)
        {
            return NotFound();
        }

        if (formdata.Answer != TaskStatusEnum.Completed && formdata.Answer != TaskStatusEnum.InProgress)
        {
            return BadRequest();
        }

        bool hasTaskStatusUpdated = await _taskService.UpdateTaskStatus(application.ApplicationId, questionDetails.TaskId, formdata.Answer);

        if (!hasTaskStatusUpdated)
        {
            return BadRequest();
        }

        return Redirect(RouteConstants.ApplicationConstants.TASK_LIST_PATH);
    }
}