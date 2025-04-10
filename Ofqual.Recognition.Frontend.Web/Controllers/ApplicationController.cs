﻿using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Web.Mappers;
using GovUk.Frontend.AspNetCore.TagHelpers;

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

        var domainItems = await _taskService.GetApplicationTasks(application.ApplicationId);

        TaskListViewModel viewModel = TaskListMapper.MapToViewModel(domainItems);
        
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

    [HttpGet("review-submit/review-your-application")]
    public async Task<IActionResult> ApplicationReview()
    {
        Application? application = _sessionService.GetFromSession<Application>(SessionKeys.Application);

        if (application == null)
        {
            // TODO: Redirect to login page and not home page
            return RedirectToAction("Home");
        }

        var reviewModel = new TaskReviewViewModel()
        {
            Answer = TaskStatusEnum.InProgress,            
            SectionHeadings =
            [
                new TaskReviewSectionViewModel
                {
                    Title = "Application Details",
                    Items =
                    [
                        new TaskReviewItemViewModel
                        {   Title = "Contact details",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel(){ Question = "Full name" },
                                new TaskReviewQuestionAnswerViewModel(){ Question = "Email address" },
                                new TaskReviewQuestionAnswerViewModel(){ Question = "Phone number" },
                                new TaskReviewQuestionAnswerViewModel(){ Question = "Your role in the organisation" }
                            ]
                        },
                        new TaskReviewItemViewModel { Title = "Organisation details",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel() { Question = "Organisation name" },
                                new TaskReviewQuestionAnswerViewModel() { Question = "Organisation legal name" },
                                new TaskReviewQuestionAnswerViewModel() { Question = "Acronym" },
                                new TaskReviewQuestionAnswerViewModel() { Question = "Website" }
                            ]
                         },
                        new TaskReviewItemViewModel { Title = "Organisation address" },
                        new TaskReviewItemViewModel { Title = "Qualifications or end-point assessments" },
                        new TaskReviewItemViewModel { Title = "Ofqual recognition",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel { Question = "Why do you want to be regulated by Ofqual?" },
                            ]
                        },
                    ]
                },
                new TaskReviewSectionViewModel
                {
                    Title = "Criteria A: Identity, constitution and governance",
                    Items =
                    [
                        new TaskReviewItemViewModel { Title = "Criteria A.1, A.2 and A.3 - Identity and Constitution" },
                        new TaskReviewItemViewModel { Title = "Criteria A.4 - Organisation and governance" },
                        new TaskReviewItemViewModel { Title = "Criteria A.5 - Conflicts of interest" },
                        new TaskReviewItemViewModel { Title = "Criteria A.6 - Governing body oversight" },
                        new TaskReviewItemViewModel { Title = "Criteria A - Uploaded files" }
                    ]
                },
                new TaskReviewSectionViewModel
                {
                    Title = "Criteria B: Integrity",
                    Items = [
                        new TaskReviewItemViewModel { Title = "Criteria B.1 - Integrity of the applicant" },
                        new TaskReviewItemViewModel { Title = "Criteria B.2 - Integrity of senior officers" },
                        new TaskReviewItemViewModel { Title = "Criteria B information" },
                        new TaskReviewItemViewModel { Title = "Criteria B - Uploaded files" }
                    ]
                },
                new TaskReviewSectionViewModel
                {
                    Title = "Criteria C: Resources and Financing",
                    Items =
                    [
                        new TaskReviewItemViewModel { Title = "Criterion C.1(a) - Systems, processes and resources" },
                        new TaskReviewItemViewModel { Title = "Criterion C.1(b) - Financial resources and facilities" },
                        new TaskReviewItemViewModel { Title = "Criteria C - Uploaded files" }
                    ]
                },
                new TaskReviewSectionViewModel
                {
                    Title = "Criteria D: Competence",
                    Items =
                    [
                        new TaskReviewItemViewModel { Title = "Criterion D.1(a) - Development, delivery and awarding of qualifications" },
                        new TaskReviewItemViewModel { Title = "Criterion D.1(b) - Validity, Reliability, Comparability, Manageability and Minimising Bias" },
                        new TaskReviewItemViewModel { Title = "Criterion D.1(c) - Qualification compatibility with Equalities Law" },
                        new TaskReviewItemViewModel { Title = "Criteria D - Uploaded files" }
                    ]
                }
            ]               
        };

        return await Task.FromResult(View(reviewModel));
    }
}
