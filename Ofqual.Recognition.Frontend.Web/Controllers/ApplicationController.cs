using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.ViewModels;
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

    [HttpGet]
    public async Task<IActionResult> StartApplication()
    {
        Application? application = await _applicationService.SetUpApplication();

        if (application == null)
        {
            // TODO: Redirect to error page or login page?
            return RedirectToAction("Home");
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
    public IActionResult TaskReview()
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        return View();
    }

    [HttpPost("review-your-task-answers")]
    public async Task<IActionResult> TaskReview(Guid taskId, TaskReview model)
    {
        var application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        if (model.Answer != TaskStatusEnum.Completed && model.Answer != TaskStatusEnum.InProgress)
        {
            return RedirectToAction("TaskReview");
        }

        await _taskService.UpdateTaskStatus(application.ApplicationId, taskId, model.Answer);

        return RedirectToAction("TaskList");
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
}
