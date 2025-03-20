using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("application")]
public class ApplicationController : Controller
{
    private readonly IApplicationService _applicationService;
    private readonly ITaskService _taskService;
    private readonly ISessionService _sessionService;

    public ApplicationController(IApplicationService applicationService, ITaskService taskService, ISessionService sessionService)
    {
        _applicationService = applicationService;
        _taskService = taskService;
        _sessionService = sessionService;
    }

    public async Task<IActionResult> StartApplication()
    {
        Application? application = await _applicationService.SetUpApplication();

        if (application == null)
        {
            // TODO: Redirect to error page or login page?
            return RedirectToAction("Error", "Home");
        }

        return RedirectToAction("TaskList");
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> TaskList()
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        var tasks = await _taskService.GetApplicationTasks(application.ApplicationId);

        return View(tasks);
    }

    [HttpGet("review-your-task-answers")]
    public IActionResult TaskReview(Guid taskId)
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        ViewBag.TaskId = taskId;
        return View();
    }

    [HttpGet("review-your-application-answers")]
    public IActionResult ApplicationReview()
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        return View();
    }

    [HttpPost("submit-answer")]
    public async Task<IActionResult> SubmitApplicationAnswer(Guid taskId, [FromForm] string completed)
    {
        // TODO: This needs to use a viewmodel
        
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        if (completed == "Yes")
        {
            await _taskService.UpdateTaskStatus(application.ApplicationId, taskId, TaskStatusEnum.Completed);
        }
        else
        {
            await _taskService.UpdateTaskStatus(application.ApplicationId, taskId, TaskStatusEnum.InProgress);
        }

        return RedirectToAction("TaskList");
    }
}
