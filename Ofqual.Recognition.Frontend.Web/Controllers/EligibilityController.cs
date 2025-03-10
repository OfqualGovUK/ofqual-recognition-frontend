using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Web.Models;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    public class EligibilityController : Controller
    {
        public IActionResult Start() => View();

        public IActionResult QuestionOne() => View();

        [HttpPost]
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

        public IActionResult QuestionTwo() => View();

        [HttpPost]
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

        public IActionResult QuestionThree() => View();

        [HttpPost]
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

        public IActionResult QuestionCheck()
        {
            var model = new EligibilityModel
            {
                QuestionOne = TempData.Peek("QuestionOne") as string ?? string.Empty,
                QuestionTwo = TempData.Peek("QuestionTwo") as string ?? string.Empty,
                QuestionThree = TempData.Peek("QuestionThree") as string ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult QuestionSubmit(string questionThree)
        {
            var model = new EligibilityModel
            {
                QuestionOne = TempData["QuestionOne"] as string ?? string.Empty,
                QuestionTwo = TempData["QuestionTwo"]as string ?? string.Empty,
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

        public IActionResult Eligible() => View();

        public IActionResult NotEligible() => View();
    }
}