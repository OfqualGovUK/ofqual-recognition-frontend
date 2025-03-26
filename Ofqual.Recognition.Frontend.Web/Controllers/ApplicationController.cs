using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Web.Mappers;

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
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        return RedirectToAction("TaskList");
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> TaskList()
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        var domainSections = await _taskService.GetApplicationTasks(application.ApplicationId);

        TaskListViewModel viewModel = TaskListMapper.MapToViewModel(domainSections);
        
        return View(viewModel);
    }

    [HttpGet("review-your-task-answers")]
    public IActionResult TaskReview()
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        return View();
    }

    [HttpPost("review-your-task-answers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TaskReview(Guid taskId, TaskReviewViewModel model)
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

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
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        return View();
    }
}
