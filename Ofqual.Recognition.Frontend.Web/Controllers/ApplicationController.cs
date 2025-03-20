using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Models;

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
        Application? application = _sessionService.GetApplication();

        if (application == null) 
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        var tasks = await _taskService.GetApplicationTasks(application.ApplicationId);

        return View(tasks);
    }

    [HttpGet("review-your-task-answers")]
    public async Task<IActionResult> TaskReview(Guid taskId)
    {
        Application? application = _sessionService.GetApplication();

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        // TODO: Check user progress instead of hard coding completed status
        await _taskService.UpdateTaskStatus(application.ApplicationId, taskId, TaskStatusEnum.Completed);

        return View();
    }

    [HttpGet("review-your-application-answers")]
    public async Task<IActionResult> ApplicationReview(Guid taskId)
    {
        Application? application = _sessionService.GetApplication();

        if (application == null) 
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        // TODO: Check user progress instead of hard coding completed status
        await _taskService.UpdateTaskStatus(application.ApplicationId, taskId, TaskStatusEnum.Completed);

        return View();
    }
}
