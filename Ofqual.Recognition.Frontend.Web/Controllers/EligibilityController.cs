﻿using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    [Route("eligibility")]
    public class EligibilityController : Controller
    {
        private readonly IEligibilityService _eligibilityService;
        private readonly ILogger<EligibilityController> _logger;

        public EligibilityController(IEligibilityService eligibilityService, ILogger<EligibilityController> logger)
        {
            _eligibilityService = eligibilityService;
            _logger = logger;
        }

        [HttpGet("start")]
        public IActionResult Start() => View();

        [HttpGet("question-one")]
        public IActionResult QuestionOne()
        {
            _logger.LogInformation("Getting answers for QuestionOne.");

            return View(_eligibilityService.GetAnswers());
        }

        [HttpPost("question-one")]
        public IActionResult QuestionOne(string questionOne)
        {
            if (string.IsNullOrWhiteSpace(questionOne))
            {
                _logger.LogWarning("Invalid input: QuestionOne is empty.");
                ModelState.AddModelError("", "You need to select an option to continue.");

                return View(_eligibilityService.GetAnswers());
            }

            _logger.LogInformation("Saving answer for QuestionOne: {questionOne}", questionOne);
            _eligibilityService.SaveAnswers(questionOne, null, null);

            return RedirectToAction("QuestionTwo");
        }

        [HttpGet("question-two")]
        public IActionResult QuestionTwo()
        {
            _logger.LogInformation("Getting answers for QuestionTwo.");

            return View(_eligibilityService.GetAnswers());
        }

        [HttpPost("question-two")]
        public IActionResult QuestionTwo(string questionTwo)
        {
            if (string.IsNullOrWhiteSpace(questionTwo))
            {
                _logger.LogWarning("Invalid input: QuestionTwo is empty.");
                ModelState.AddModelError("", "You need to select an option to continue.");

                return View(_eligibilityService.GetAnswers());
            }

            _logger.LogInformation("Saving answer for QuestionTwo: {questionTwo}", questionTwo);
            _eligibilityService.SaveAnswers(null, questionTwo, null);

            return RedirectToAction("QuestionThree");
        }

        [HttpGet("question-three")]
        public IActionResult QuestionThree()
        {
            _logger.LogInformation("Getting answers for QuestionThree.");

            return View(_eligibilityService.GetAnswers());
        }

        [HttpPost("question-three")]
        public IActionResult QuestionThree(string questionThree)
        {
            if (string.IsNullOrWhiteSpace(questionThree))
            {
                _logger.LogWarning("Invalid input: QuestionThree is empty.");
                ModelState.AddModelError("", "You need to select an option to continue.");

                return View(_eligibilityService.GetAnswers());
            }

            _logger.LogInformation("Saving answer for QuestionThree: {questionThree}", questionThree);
            _eligibilityService.SaveAnswers(null, null, questionThree);

            return RedirectToAction("QuestionCheck");
        }

        [HttpGet("check")]
        public IActionResult QuestionCheck()
        {
            return View(_eligibilityService.GetAnswers());
        }

        [HttpPost("submit")]
        public IActionResult QuestionSubmit(string questionThree)
        {
            var model = _eligibilityService.GetAnswers();

            if (model.QuestionOne == "Yes" && model.QuestionTwo == "Yes" && model.QuestionThree == "Yes")
            { 
                return RedirectToAction("Eligible");
            }

            return RedirectToAction("NotEligible");
        }

        [HttpGet("eligible")]
        public IActionResult Eligible() {
            HttpContext.Session.Clear();

            return View();
        }

        [HttpGet("not-eligible")]
        public IActionResult NotEligible() 
        {
            return View(_eligibilityService.GetAnswers());
        }
    }
}