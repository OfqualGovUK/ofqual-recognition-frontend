using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    public class EligibilityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult QuestionOne()
        {
            return View();
        }

        public IActionResult QuestionTwo()
        {
            return View();
        }

        public IActionResult QuestionThree()
        {
            return View();
        }

        public IActionResult Eligible()
        {
            return View();
        }

        public IActionResult NotEligible()
        {
            return View();
        }
    }
}
