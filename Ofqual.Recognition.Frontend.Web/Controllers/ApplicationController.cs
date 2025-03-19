using Microsoft.AspNetCore.Mvc;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    [Route("application")]
    public class ApplicationController : Controller
    {
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(ILogger<ApplicationController> logger)
        { 
            _logger = logger;
        }

        [HttpGet("tasks")]
        public IActionResult TaskList()
        {
            // attempt to get an applicationId via creation or cache

            // get task list with statuses

            // display tasks in their sections
            return View(model);
        }

        [HttpGet("check-your-answers")]
        public IActionResult TaskCheck() {
            // add ?taskId={taskId} to url check-your-answers?taskId={taskId}
            return View();
        }
    }
}
