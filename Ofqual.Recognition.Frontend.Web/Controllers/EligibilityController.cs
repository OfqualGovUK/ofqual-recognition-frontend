using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Infrastructure.Service.Interfaces;
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

        return View(new QuestionOneViewModel
        {
            Answer = model.Answer
        });
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

        return View(new QuestionTwoViewModel
        {
            Answer = model.Answer
        });
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

        return View(new QuestionThreeViewModel
        {
            Answer = model.Answer
        });
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

        return View(new EligibilityViewModel
        {
            QuestionOne = model.QuestionOne,
            QuestionTwo = model.QuestionTwo,
            QuestionThree = model.QuestionThree
        });
    }

    [HttpPost("submit")]
    public IActionResult QuestionSubmit()
    {
        Eligibility model = _eligibilityService.GetAnswers();

        if (model.QuestionOne == "Yes" && model.QuestionTwo == "Yes" && model.QuestionThree == "Yes")
        {
            return RedirectToAction("Eligible");
        }

        return RedirectToAction("NotEligible");
    }

    [HttpGet("eligible")]
    public IActionResult Eligible()
    {
        Eligibility model = _eligibilityService.GetAnswers();

        if (model.QuestionOne != "Yes" && model.QuestionTwo != "Yes" && model.QuestionThree != "Yes")
        {
            return RedirectToAction("Start");
        }

        HttpContext.Session.Clear();

        return View();
    }

    [HttpGet("not-eligible")]
    public IActionResult NotEligible()
    {
        Eligibility model = _eligibilityService.GetAnswers();

        return View(new EligibilityViewModel
        {
            QuestionOne = model.QuestionOne,
            QuestionTwo = model.QuestionTwo,
            QuestionThree = model.QuestionThree
        });
    }
}