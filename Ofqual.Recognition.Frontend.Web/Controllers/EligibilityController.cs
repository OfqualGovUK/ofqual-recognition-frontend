﻿using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Ofqual.Recognition.Frontend.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("eligibility")]
public class EligibilityController(IEligibilityService eligibilityService, ISessionService sessionService, IFeatureFlagService featureFlagService) : Controller
{
    private readonly IEligibilityService _eligibilityService = eligibilityService;
    private readonly ISessionService _sessionService = sessionService;
    private readonly IFeatureFlagService _featureFlagService = featureFlagService;

    [HttpGet("start")]
    public IActionResult Start()
    {
        return View();
    }

    [HttpGet("question-one")]
    public IActionResult QuestionOne(string? returnUrl)
    {
        EligibilityQuestion model = _eligibilityService.GetQuestion(SessionKeys.EligibilityQuestionOne);

        QuestionOneViewModel viewModel = EligibilityMapper.MapToQuestionOneViewModel(model);
        viewModel.ReturnUrl = returnUrl;

        return View(viewModel);
    }

    [HttpPost("question-one")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionOne(QuestionOneViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _sessionService.SetInSession(SessionKeys.EligibilityQuestionOne, model.Answer);

        return !string.IsNullOrEmpty(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("QuestionTwo");
    }

    [HttpGet("question-two")]
    public IActionResult QuestionTwo(string? returnUrl)
    {
        EligibilityQuestion model = _eligibilityService.GetQuestion(SessionKeys.EligibilityQuestionTwo);

        QuestionTwoViewModel viewModel = EligibilityMapper.MapToQuestionTwoViewModel(model);
        viewModel.ReturnUrl = returnUrl;

        return View(viewModel);
    }

    [HttpPost("question-two")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionTwo(QuestionTwoViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _sessionService.SetInSession(SessionKeys.EligibilityQuestionTwo, model.Answer);

        return !string.IsNullOrEmpty(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("QuestionThree");
    }

    [HttpGet("question-three")]
    public IActionResult QuestionThree(string? returnUrl)
    {
        EligibilityQuestion model = _eligibilityService.GetQuestion(SessionKeys.EligibilityQuestionThree);

        QuestionThreeViewModel viewModel = EligibilityMapper.MapToQuestionThreeViewModel(model);
        viewModel.ReturnUrl = returnUrl;

        return View(viewModel);
    }

    [HttpPost("question-three")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionThree(QuestionThreeViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _sessionService.SetInSession(SessionKeys.EligibilityQuestionThree, model.Answer);

        return !string.IsNullOrEmpty(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("QuestionReview");
    }

    [HttpGet("question-review")]
    public IActionResult QuestionReview()
    {
        Eligibility model = _eligibilityService.GetAnswers();

        EligibilityViewModel viewModel = EligibilityMapper.MapToEligibilityViewModel(model);

        return View(viewModel);
    }

    [HttpPost("submit")]
    public IActionResult QuestionSubmit()
    {
        Eligibility model = _eligibilityService.GetAnswers();

        if (model.QuestionOne != "Yes" || model.QuestionTwo != "Yes" || model.QuestionThree != "Yes")
        {
            return RedirectToAction("NotEligible");
        }

        return RedirectToAction("Eligible");
    }

    [HttpGet("eligible")]
    public IActionResult Eligible()
    {
        Eligibility model = _eligibilityService.GetAnswers();

        if (model.QuestionOne != "Yes" && model.QuestionTwo != "Yes" && model.QuestionThree != "Yes")
        {
            return RedirectToAction("Start");
        }

        bool eligibilityLinkFlag = _featureFlagService.IsFeatureEnabled("EligibilityLink");

        ViewData["EligibilityLinkFlag"] = eligibilityLinkFlag;

        return View();
    }

    [HttpGet("not-eligible")]
    public IActionResult NotEligible()
    {
        Eligibility model = _eligibilityService.GetAnswers();

        EligibilityViewModel viewModel = EligibilityMapper.MapToEligibilityViewModel(model);



        return View(viewModel);
    }
}