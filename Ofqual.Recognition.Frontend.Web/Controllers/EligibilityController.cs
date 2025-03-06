using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Web.Models;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    public class EligibilityController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult QuestionOne() => View();

        [HttpPost]
        public IActionResult QuestionOne(string questionOne)
        {
            if (string.IsNullOrEmpty(questionOne))
            {
                ModelState.AddModelError("QuestionOne", "Please select an answer.");
                return View();
            }

            TempData["QuestionOne"] = questionOne;

            return RedirectToAction("QuestionTwo");
        }

        public IActionResult QuestionTwo() => View();

        [HttpPost]
        public IActionResult QuestionTwo(string questionTwo)
        {
            if (string.IsNullOrEmpty(questionTwo))
            {
                ModelState.AddModelError("QuestionTwo", "Please select an answer.");
                return View();
            }
            TempData["QuestionTwo"] = questionTwo;

            return RedirectToAction("QuestionThree");
        }

        public IActionResult QuestionThree() => View();

        [HttpPost]
        public IActionResult QuestionThree(string questionThree)
        {
            if (string.IsNullOrEmpty(questionThree))
            {
                ModelState.AddModelError("QuestionThree", "Please select an answer.");
                return View();
            }
            TempData["QuestionThree"] = questionThree;

            return RedirectToAction("QuestionCheck");
        }

        public IActionResult QuestionCheck()
        {
            var model = new EligibilityModel
            {
                QuestionOne = TempData["QuestionOne"] as string,
                QuestionTwo = TempData["QuestionTwo"] as string,
                QuestionThree = TempData["QuestionThree"] as string
            };

            TempData.Keep();

            return View(model);
        }

        [HttpPost]
        public IActionResult QuestionSubmit(string questionThree)
        {
            var model = new EligibilityModel
            {
                QuestionOne = TempData["QuestionOne"] as string,
                QuestionTwo = TempData["QuestionTwo"] as string,
                QuestionThree = TempData["QuestionThree"] as string
            };

            TempData.Keep();

            if (model.IsEligible())
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