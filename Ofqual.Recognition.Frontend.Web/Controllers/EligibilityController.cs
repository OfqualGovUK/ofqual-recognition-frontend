using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.ViewModels;
using Ofqual.Recognition.Frontend.Infrastructure.Service.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

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
        var model = _eligibilityService.GetQuestion<QuestionOne>(SessionKeys.QuestionOne);

        return View(model);
    }

    [HttpPost("question-one")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionOne(QuestionOne model, string? returnUrl)
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
        var model = _eligibilityService.GetQuestion<QuestionTwo>(SessionKeys.QuestionTwo);

        return View(model);
    }

    [HttpPost("question-two")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionTwo(QuestionTwo model, string? returnUrl)
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
        var model = _eligibilityService.GetQuestion<QuestionThree>(SessionKeys.QuestionThree);

        return View(model);
    }

    [HttpPost("question-three")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionThree(QuestionThree model)
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
        var model = _eligibilityService.GetAnswers();
        return View(model);
    }

    [HttpPost("submit")]
    public IActionResult QuestionSubmit()
    {
        var model = _eligibilityService.GetAnswers();

        if (model.QuestionOne == "Yes" && model.QuestionTwo == "Yes" && model.QuestionThree == "Yes")
        {
            return RedirectToAction("Eligible");
        }

        return RedirectToAction("NotEligible");
    }

    [HttpGet("eligible")]
    public IActionResult Eligible()
    {
        var model = _eligibilityService.GetAnswers();

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
        var model = _eligibilityService.GetAnswers();
        return View(model);
    }
}