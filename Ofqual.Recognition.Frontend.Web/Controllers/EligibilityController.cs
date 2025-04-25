using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using System.Net.Http.Headers;

namespace Ofqual.Recognition.Frontend.Web.Controllers;

[Route("eligibility")]
[Authorize]
public class EligibilityController : Controller
{
    private readonly IEligibilityService _eligibilityService;
    private readonly ILogger<EligibilityController> _logger;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;
    private const string ServiceName = "CitizenAPI";

    public EligibilityController(IEligibilityService eligibilityService, ILogger<EligibilityController> logger, ITokenAcquisition tokenAcquisition, IConfiguration configuration)
    {
        _eligibilityService = eligibilityService;
        _logger = logger;
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;
    }

    [AuthorizeForScopes(ScopeKeySection = "DownstreamApis:CitizenAPI:Scopes")]
    [HttpGet("start")]
    public async Task<IActionResult> StartAsync() 
    {
        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_configuration.GetSection("DownstreamApis:CitizenAPI:Scopes").Get<IEnumerable<string>>());
        _logger.LogInformation("Access token: {accessToken}", accessToken); // Not for production!

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await client.GetAsync("https://localhost:7037/questions/application-details/qualifications");
        }

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