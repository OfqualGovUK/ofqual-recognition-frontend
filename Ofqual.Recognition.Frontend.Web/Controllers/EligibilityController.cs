using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Models;

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

        QuestionOneViewModel questionOne = new QuestionOneViewModel
        {
            Answer = model.Answer
        };

        return View(questionOne);
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

        QuestionTwoViewModel questionTwo = new QuestionTwoViewModel
        {
            Answer = model.Answer
        };

        return View(questionTwo);
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

        QuestionThreeViewModel questionThree = new QuestionThreeViewModel
        {
            Answer = model.Answer
        };

        return View(questionThree);
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

        return RedirectToAction("QuestionCheck");
    }

    [HttpGet("check")]
    public IActionResult QuestionCheck()
    {
        Eligibility model = _eligibilityService.GetAnswers();

        EligibilityViewModel eligibility = new EligibilityViewModel
        {
            QuestionOne = model.QuestionOne,
            QuestionTwo = model.QuestionTwo,
            QuestionThree = model.QuestionThree
        };

        return View(eligibility);
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

        EligibilityViewModel eligibility = new EligibilityViewModel
        {
            QuestionOne = model.QuestionOne,
            QuestionTwo = model.QuestionTwo,
            QuestionThree = model.QuestionThree
        };

        return View(eligibility);
    }
}