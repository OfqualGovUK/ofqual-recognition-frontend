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
            TempData["QuestionOne"] = questionOne;

            return RedirectToAction("QuestionTwo");
        }

        public IActionResult QuestionTwo() => View();

        [HttpPost]
        public IActionResult QuestionTwo(string questionTwo)
        {
            TempData["QuestionTwo"] = questionTwo;

            return RedirectToAction("QuestionThree");
        }

        public IActionResult QuestionThree() => View();

        [HttpPost]
        public IActionResult QuestionThree(string questionThree)
        {
            TempData["QuestionThree"] = questionThree;

            var model = new EligibilityModel
            {
                QuestionOne = TempData["QuestionOne"] as string,
                QuestionTwo = TempData["QuestionTwo"] as string,
                QuestionThree = questionThree
            };

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