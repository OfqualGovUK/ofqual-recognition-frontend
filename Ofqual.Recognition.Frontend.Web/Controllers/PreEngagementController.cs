using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[AllowAnonymous]
[Route("pre-engagement")]
public class PreEngagementController : Controller
{
    private readonly IPreEngagementService _preEngagementService;
    private readonly IMemoryCacheService _memoryCacheService;

    public PreEngagementController(IPreEngagementService preEngagementService, IMemoryCacheService memoryCacheService)
    {
        _preEngagementService = preEngagementService;
        _memoryCacheService = memoryCacheService;
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
        PreEngagementQuestionDetails? questionDetails = await _preEngagementService.GetPreEngagementQuestionDetails(taskNameUrl, questionNameUrl);

        if (questionDetails == null)
        {
            return NotFound();
        }

        var preEngagement = _memoryCacheService.GetFromCache<List<PreEngagementAnswer>>(SessionKeys.PreEngagementAnswers);
        var currentQuestionAnswer = preEngagement?.FirstOrDefault(a => a.QuestionId == questionDetails.QuestionId && a.TaskId == questionDetails.TaskId);
  
        QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);
        questionViewModel.AnswerJson = currentQuestionAnswer?.AnswerJson;
        questionViewModel.FromPreEngagement = true;

        return View("~/Views/Application/QuestionDetails.cshtml", questionViewModel);
    }

    [HttpPost("{taskNameUrl}/{questionNameUrl}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreEngagementSubmitAnswers(string taskNameUrl, string questionNameUrl, [FromForm] IFormCollection formdata)
    {
        PreEngagementQuestionDetails? questionDetails = await _preEngagementService.GetPreEngagementQuestionDetails(taskNameUrl, questionNameUrl);

        if (questionDetails == null)
        {
            return NotFound();
        }

        string jsonAnswer = FormDataHelper.ConvertToJson(formdata);
        _memoryCacheService.UpsertPreEngagementAnswer(questionDetails.QuestionId, questionDetails.TaskId, jsonAnswer);

        if (string.IsNullOrEmpty(questionDetails.NextQuestionUrl))
        {
            return RedirectToAction("StartApplication", "Application");
        }

        var next = QuestionUrlHelper.Parse(questionDetails.NextQuestionUrl);
        if (next == null)
        {
            return BadRequest("Invalid next question URL.");
        }

        return RedirectToAction(nameof(PreEngagementQuestionDetails), new
        {
            next.Value.taskNameUrl,
            next.Value.questionNameUrl
        });
    }
}
