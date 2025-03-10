using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Web.Models;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    [Route("eligibility")]
    public class EligibilityController : Controller
    {
        [HttpGet("start")]
        public IActionResult Start() => View();

        [HttpGet("question-one")]
        public IActionResult QuestionOne() => View();

        [HttpPost("question-one")]
        public IActionResult QuestionOne(string questionOne)
        {
            if (string.IsNullOrWhiteSpace(questionOne))
            {
                ModelState.AddModelError("", "You need to select an option to continue.");
                return View();
            }

            TempData["QuestionOne"] = questionOne;

            return RedirectToAction("QuestionTwo");
        }

        [HttpGet("question-two")]
        public IActionResult QuestionTwo() => View();

        [HttpPost("question-two")]
        public IActionResult QuestionTwo(string questionTwo)
        {
            if (string.IsNullOrWhiteSpace(questionTwo))
            {
                ModelState.AddModelError("", "You need to select an option to continue.");
                return View();
            }

            TempData["QuestionTwo"] = questionTwo;

            return RedirectToAction("QuestionThree");
        }

        [HttpGet("question-three")]
        public IActionResult QuestionThree() => View();

        [HttpPost("question-three")]
        public IActionResult QuestionThree(string questionThree)
        {
            if (string.IsNullOrWhiteSpace(questionThree))
            {
                ModelState.AddModelError("", "You need to select an option to continue.");
                return View();
            }

            TempData["QuestionThree"] = questionThree;

            return RedirectToAction("QuestionCheck");
        }

        [HttpGet("check")]
        public IActionResult QuestionCheck()
        {
            var model = new EligibilityModel
            {
                QuestionOne = TempData.Peek("QuestionOne") as string ?? string.Empty,
                QuestionTwo = TempData.Peek("QuestionTwo") as string ?? string.Empty,
                QuestionThree = TempData.Peek("QuestionThree") as string ?? string.Empty
            };

            TempData.Keep();

            return View(model);
        }

        [HttpPost("submit")]
        public IActionResult QuestionSubmit(string questionThree)
        {
            var model = new EligibilityModel
            {
                QuestionOne = TempData["QuestionOne"] as string ?? string.Empty,
                QuestionTwo = TempData["QuestionTwo"] as string ?? string.Empty,
                QuestionThree = TempData["QuestionThree"] as string ?? string.Empty
            };

            TempData.Keep();

            if (model.QuestionOne == "Yes" && model.QuestionTwo == "Yes" && model.QuestionThree == "Yes")
            {
                return RedirectToAction("Eligible");
            }
            else
            {
                return RedirectToAction("NotEligible");
            }
        }

        [HttpGet("eligible")]
        public IActionResult Eligible() => View();

        [HttpGet("not-eligible")]
        public IActionResult NotEligible() 
        {
            var model = new EligibilityModel
            {
                QuestionOne = TempData["QuestionOne"] as string ?? string.Empty,
                QuestionTwo = TempData["QuestionTwo"] as string ?? string.Empty,
                QuestionThree = TempData["QuestionThree"] as string ?? string.Empty
            };

            TempData.Keep();

            return View(model);
        }
    }
}