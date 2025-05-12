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
    public async Task<IActionResult> QuestionDetails(AnswerHelper answerHelper, string taskNameUrl, string questionNameUrl, bool fromReview = false)
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
            return RedirectToAction(nameof(TaskReview), new
            {
                taskNameUrl
            });
        }

        var mappedData = answerHelper.MapQuestionAnswer(questionDetails, questionAnswer);

        QuestionViewModel questionViewModel = new QuestionViewModel
        {
            QuestionId = mappedData.questionDetails.QuestionId,
            TaskId = mappedData.questionDetails.TaskId,
            QuestionTypeName = mappedData.questionDetails.QuestionTypeName,
            QuestionContent = MapToQuestionContentViewModel(mappedData.questionDetails),
            CurrentQuestionUrl = mappedData.questionDetails.CurrentQuestionUrl,
            PreviousQuestionUrl = mappedData.questionDetails.PreviousQuestionUrl,
            FromReview = fromReview,
        };

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

        if (questionAnswerResult == null || questionAnswerResult.NextQuestionNameUrl == null || questionAnswerResult.NextTaskNameUrl == null)
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
    public async Task<IActionResult> TaskReview(string taskNameUrl, AnswerHelper answerHelper)
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

        var mappedAnswers = answerHelper.MapQuestionAnswerSections(reviewAnswers);

        var taskReviewViewModel = new TaskReviewViewModel
        {
            QuestionAnswerSections = mappedAnswers.Select(section => new QuestionAnswerSectionViewModel
            {
                SectionHeading = section.SectionHeading,
                QuestionAnswers = section.QuestionAnswers.Select(answer => new QuestionAnswerReviewViewModel
                {
                    QuestionText = answer.QuestionText,
                    AnswerValue = answer.AnswerValue,
                    QuestionUrl = answer.QuestionUrl
                }).ToList()
            }).ToList()
        };

        TaskStatusEnum? status = _sessionService.GetTaskStatusFromSession(taskDetails.TaskId);

        if (status == null)
        {
            return BadRequest();
        }

        return View(taskReviewViewModel);
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

        return Redirect(RouteConstants.ApplicationConstants.TASK_LIST_PATH);
    }

    private QuestionContentViewModel MapToQuestionContentViewModel(QuestionDetails questionDetails) 
    { 
        return new QuestionContentViewModel
        {
            Heading = questionDetails.QuestionContent,
            Body = new List<BodyItemViewModel>(),
            Help = new List<HelpItemViewModel>(),
            FormGroup = new FormGroupViewModel()
        };
    }
}