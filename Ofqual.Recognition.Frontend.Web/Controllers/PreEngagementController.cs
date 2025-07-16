using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[AllowAnonymous]
[Route("pre-engagement")]
public class PreEngagementController : Controller
{
    private readonly IPreEngagementService _preEngagementService;
    private readonly ISessionService _sessionService;
    private readonly IApplicationService _applicationService;

    public PreEngagementController(IPreEngagementService preEngagementService, ISessionService sessionService, IApplicationService applicationService)
    {
        _preEngagementService = preEngagementService;
        _sessionService = sessionService;
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<IActionResult> StartPreEngagement()
    {
        PreEngagementQuestion? firstQuestion = await _preEngagementService.GetFirstPreEngagementQuestion();

        if (firstQuestion == null)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(PreEngagementQuestionDetails), new
        {
            taskNameUrl = firstQuestion.CurrentTaskNameUrl,
            questionNameUrl = firstQuestion.CurrentQuestionNameUrl
        });
    }

    [HttpGet("{taskNameUrl}/{questionNameUrl}")]
    public async Task<IActionResult> PreEngagementQuestionDetails(string taskNameUrl, string questionNameUrl)
    {
        QuestionDetails? questionDetails = await _preEngagementService.GetPreEngagementQuestionDetails(taskNameUrl, questionNameUrl);

        if (questionDetails == null)
        {
            return NotFound();
        }

        var preEngagement = _sessionService.GetFromSession<List<PreEngagementAnswer>>(SessionKeys.PreEngagementAnswers);
        var currentQuestionAnswer = preEngagement?.FirstOrDefault(a => a.QuestionId == questionDetails.QuestionId && a.TaskId == questionDetails.TaskId);

        QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);
        questionViewModel.AnswerJson = currentQuestionAnswer?.AnswerJson;
        questionViewModel.FromPreEngagement = true;

        return View("~/Views/Application/QuestionDetails.cshtml", questionViewModel);
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreEngagementQuestionDetails(string taskNameUrl, string questionNameUrl, [FromForm] IFormCollection formdata)
    {
        QuestionDetails? questionDetails = await _preEngagementService.GetPreEngagementQuestionDetails(taskNameUrl, questionNameUrl);

        if (questionDetails == null)
        {
            return NotFound();
        }

        string jsonAnswer = JsonHelper.ConvertToJson(formdata);

        ValidationResponse? validationResponse = await _preEngagementService.ValidatePreEngagementAnswer(questionDetails.QuestionId, jsonAnswer);
        if (validationResponse == null)
        {
            return BadRequest();
        }

        if (validationResponse.Errors != null && validationResponse.Errors.Any())
        {
            QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);
            questionViewModel.Validation = QuestionMapper.MapToViewModel(validationResponse);
            questionViewModel.FromPreEngagement = true;
            questionViewModel.AnswerJson = jsonAnswer;

            return View("~/Views/Application/QuestionDetails.cshtml", questionViewModel);
        }

        _sessionService.UpsertPreEngagementAnswer(questionDetails.QuestionId, questionDetails.TaskId, jsonAnswer);

        if (string.IsNullOrEmpty(questionDetails.NextQuestionUrl))
        {
            return RedirectToAction(nameof(ApplicationController.InitialiseApplication), "Application");
        }

        return RedirectToAction(nameof(PreEngagementQuestionDetails), new
        {
            QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl)!.Value.taskNameUrl,
            QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl)!.Value.questionNameUrl
        });
    }

    [HttpGet("confirmation")]
    [Authorize]
    [AuthorizeForScopes(ScopeKeySection = "RecognitionApi:Scopes")]
    public async Task<IActionResult> PreEngagementConfirmation()
    {
        Application? application = await _applicationService.GetLatestApplication();
        if (application == null)
        {
            return RedirectToAction(nameof(ApplicationController.InitialiseApplication), "Application");
        }

        return View();
    }
}
