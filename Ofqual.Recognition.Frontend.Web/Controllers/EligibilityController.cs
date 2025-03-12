using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    [Route("eligibility")]
    public class EligibilityController : Controller
    {
        private readonly IEligibilityService _eligibilityService;

        public EligibilityController(IEligibilityService eligibilityService)
        {
            _eligibilityService = eligibilityService;
        }

        [HttpGet("start")]
        public IActionResult Start() => View();

        [HttpGet("question-one")]
        public IActionResult QuestionOne()
        {
            var model = _eligibilityService.GetAnswers();

            return View(model);
        }

        [HttpPost("question-one")]
        public IActionResult QuestionOne(string questionOne)
        {
            if (string.IsNullOrWhiteSpace(questionOne))
            {
                ModelState.AddModelError("", "You need to select an option to continue.");
                var model = _eligibilityService.GetAnswers();

                return View(model);
            }

            _eligibilityService.SaveAnswers(questionOne, null, null);

            return RedirectToAction("QuestionTwo");
        }

        [HttpGet("question-two")]
        public IActionResult QuestionTwo()
        {
            var model = _eligibilityService.GetAnswers();

            return View(model);
        }

        [HttpPost("question-two")]
        public IActionResult QuestionTwo(string questionTwo)
        {
            if (string.IsNullOrWhiteSpace(questionTwo))
            {
                ModelState.AddModelError("", "You need to select an option to continue.");
                var model = _eligibilityService.GetAnswers();

                return View(model);
            }

            _eligibilityService.SaveAnswers(null, questionTwo, null);

            return RedirectToAction("QuestionThree");
        }

        [HttpGet("question-three")]
        public IActionResult QuestionThree()
        {
            var model = _eligibilityService.GetAnswers();

            return View(model);
        }

        [HttpPost("question-three")]
        public IActionResult QuestionThree(string questionThree)
        {
            if (string.IsNullOrWhiteSpace(questionThree))
            {
                ModelState.AddModelError("", "You need to select an option to continue.");
                var model = _eligibilityService.GetAnswers();

                return View(model);
            }

            _eligibilityService.SaveAnswers(null, null, questionThree);

            return RedirectToAction("QuestionCheck");
        }

        [HttpGet("check")]
        public IActionResult QuestionCheck()
        {
            var model = _eligibilityService.GetAnswers();

            return View(model);
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
            var model = _eligibilityService.GetAnswers();

            return View(model);
        }
    }
}