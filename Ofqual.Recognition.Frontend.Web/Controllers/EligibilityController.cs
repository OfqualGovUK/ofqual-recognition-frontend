using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Client;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("eligibility")]
public class EligibilityController : Controller
{
    private readonly IEligibilityService _eligibilityService;
    private readonly ILogger<EligibilityController> _logger;
    private IDownstreamApi _downstreamApi;
    private const string ServiceName = "b2c-proof-of-concept-api";

    public EligibilityController(IEligibilityService eligibilityService, ILogger<EligibilityController> logger, IDownstreamApi downstreamApi)
    {
        _eligibilityService = eligibilityService;
        _logger = logger;
        _downstreamApi = downstreamApi;
        
    }

    [Authorize]
    [HttpGet("start")]
    public async Task<IActionResult> StartAsync() 
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");
        _logger.LogInformation("Access token: {accessToken}", accessToken);
        var idToken = await HttpContext.GetTokenAsync("id_token");
        _logger.LogInformation("Id token: {idToken}", idToken);
        var value = await _downstreamApi.CallApiForUserAsync(
          ServiceName,
          options =>
          {
              options.RelativePath = $"/questions/application-details/qualifications";
          });
        return View();
    } 

    [HttpGet("question-one")]
    public IActionResult QuestionOne(string returnUrl = null)
    {
        _logger.LogInformation("Getting answers for QuestionOne.");

        ViewBag.ReturnUrl = returnUrl;

        return View(_eligibilityService.GetAnswers());
    }

    [HttpPost("question-one")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionOne(string questionOne, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(questionOne))
        {
            _logger.LogWarning("Invalid input: QuestionOne is empty.");
            ModelState.AddModelError("", "You need to select an option to continue.");

            return View(_eligibilityService.GetAnswers());
        }

        _logger.LogInformation("Saving answer for QuestionOne: {questionOne}", questionOne);
        _eligibilityService.SaveAnswers(questionOne, string.Empty, string.Empty);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("QuestionTwo");
    }

    [HttpGet("question-two")]
    public IActionResult QuestionTwo(string returnUrl = null)
    {
        _logger.LogInformation("Getting answers for QuestionTwo.");

        ViewBag.ReturnUrl = returnUrl;

        return View(_eligibilityService.GetAnswers());
    }

    [HttpPost("question-two")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionTwo(string questionTwo, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(questionTwo))
        {
            _logger.LogWarning("Invalid input: QuestionTwo is empty.");
            ModelState.AddModelError("", "You need to select an option to continue.");

            return View(_eligibilityService.GetAnswers());
        }

        _logger.LogInformation("Saving answer for QuestionTwo: {questionTwo}", questionTwo);
        _eligibilityService.SaveAnswers(string.Empty, questionTwo, string.Empty);

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("QuestionThree");
    }

    [HttpGet("question-three")]
    public IActionResult QuestionThree()
    {
        _logger.LogInformation("Getting answers for QuestionThree.");

        return View(_eligibilityService.GetAnswers());
    }

    [HttpPost("question-three")]
    [ValidateAntiForgeryToken]
    public IActionResult QuestionThree(string questionThree)
    {
        if (string.IsNullOrWhiteSpace(questionThree))
        {
            _logger.LogWarning("Invalid input: QuestionThree is empty.");
            ModelState.AddModelError("", "You need to select an option to continue.");

            return View(_eligibilityService.GetAnswers());
        }

        _logger.LogInformation("Saving answer for QuestionThree: {questionThree}", questionThree);
        _eligibilityService.SaveAnswers(string.Empty, string.Empty, questionThree);

        return RedirectToAction("QuestionCheck");
    }

    [HttpGet("check")]
    public IActionResult QuestionCheck()
    {
        return View(_eligibilityService.GetAnswers());
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
        HttpContext.Session.Clear();

        return View();
    }

    [HttpGet("not-eligible")]
    public IActionResult NotEligible() 
    {
        return View(_eligibilityService.GetAnswers());
    }
}