﻿using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.Mappers;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("eligibility")]
public class EligibilityController : Controller
{
    private readonly IEligibilityService _eligibilityService;
    private readonly ISessionService _sessionService;

    public EligibilityController(IEligibilityService eligibilityService, ISessionService sessionService)
    {
        _eligibilityService = eligibilityService;
        _sessionService = sessionService;
    }

    [HttpGet("start")]
    public IActionResult Start()
    {
        return View();
    }

    [HttpGet("question-one")]
    public IActionResult QuestionOne()
    {
        Question model = _eligibilityService.GetQuestion(SessionKeys.QuestionOne);

        QuestionOneViewModel viewModel = EligibilityMapper.MapToQuestionOneViewModel(model);

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

        _sessionService.SetInSession(SessionKeys.QuestionOne, model.Answer);

        return !string.IsNullOrEmpty(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("QuestionTwo");
    }

    [HttpGet("question-two")]
    public IActionResult QuestionTwo()
    {
        Question model = _eligibilityService.GetQuestion(SessionKeys.QuestionTwo);

        QuestionTwoViewModel viewModel = EligibilityMapper.MapToQuestionTwoViewModel(model);

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

        _sessionService.SetInSession(SessionKeys.QuestionTwo, model.Answer);

        return !string.IsNullOrEmpty(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("QuestionThree");
    }

    [HttpGet("question-three")]
    public IActionResult QuestionThree()
    {
        Question model = _eligibilityService.GetQuestion(SessionKeys.QuestionThree);

        QuestionThreeViewModel viewModel = EligibilityMapper.MapToQuestionThreeViewModel(model);

        return View(viewModel);
    }

    [HttpPost("question-three")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionThree(QuestionThreeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _sessionService.SetInSession(SessionKeys.QuestionThree, model.Answer);

        return RedirectToAction("QuestionReview");
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