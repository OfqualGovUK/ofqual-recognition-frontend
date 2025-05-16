using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Ofqual.Recognition.Frontend.Web.ViewModels;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    [Route("pre-engagement")]
    public class PreEngagementController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ISessionService _sessionService;
        private readonly IMemoryCache _memoryCache;

        public PreEngagementController(ITaskService taskService, ISessionService sessionService, IMemoryCache memoryCache)
        {
            _taskService = taskService;
            _sessionService = sessionService;
            _memoryCache = memoryCache;
        }


        [HttpGet("tasks")]
        public async Task<IActionResult> TaskList()
        {
            var sessionId = Guid.NewGuid().ToString();
            TempData[SessionKeys.PreEngagementSession] = sessionId;

            var preEngagementTasks = await _taskService.GetPreEngagementTasks(sessionId);

            PreEngagement? preEngagement = _sessionService.GetFromSession<PreEngagement>(sessionId);

            var taskItemStatusSection = await _taskService.GetApplicationTasks(preEngagement.PreEngagementId);

            TaskListViewModel taskListViewModel = TaskListMapper.MapToViewModel(taskItemStatusSection);

            return View(taskListViewModel);
        }

        [HttpPost("{taskNameUrl}/{questionNameUrl}")]
        [ValidateAntiForgeryToken]
        public IActionResult SaveOrganisationInfo(QuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("QuestionDetails", model);
            }

            // Save data to in-memory cache
            var cacheKey = HttpContext.Session.Id + "_PreEngagementData";
            _memoryCache.Set(cacheKey, model, TimeSpan.FromMinutes(30));

            return RedirectToAction("ContactInfo");
        }

        [HttpGet("summary")]
        public IActionResult Summary()
        {
            var cacheKey = HttpContext.Session.Id + "_PreEngagementData";
            if (!_memoryCache.TryGetValue(cacheKey, out QuestionViewModel model))
            {
                return RedirectToAction("OrganisationInfo");
            }

            return View("Summary", model);
        }
    }
}
