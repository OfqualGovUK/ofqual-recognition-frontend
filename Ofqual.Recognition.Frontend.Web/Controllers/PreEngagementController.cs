using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Mappers;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Web.ViewModels.PreEngagement;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace Ofqual.Recognition.Frontend.Web.Controllers
{
    [Route("pre-engagement")]
    public class PreEngagementController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ISessionService _sessionService;
        private readonly IQuestionService _questionService;
        private readonly IMemoryCache _memoryCache;

        public PreEngagementController(ITaskService taskService, ISessionService sessionService, IQuestionService questionService, IMemoryCache memoryCache)
        {
            _taskService = taskService;
            _sessionService = sessionService;
            _questionService = questionService;
            _memoryCache = memoryCache;
        }

        [HttpGet("tasks")]
        public async Task<IActionResult> PreEngagementTaskDetails(string? currentTaskNameUrl = null, string? currentQuestionNameUrl = null)
        {
            var preEngagementTask = await GetPreEngagementTaskDetails(currentTaskNameUrl, currentQuestionNameUrl);

            if (preEngagementTask == null)
            {
                return NotFound();
            }

            QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(preEngagementTask.CurrentTaskNameUrl, preEngagementTask.CurrentQuestionNameUrl);

            if (questionDetails == null)
            {
                return NotFound();
            }

            QuestionViewModel questionViewModel = QuestionMapper.MapToViewModel(questionDetails);

            ViewData["CurrentTaskNameUrl"] = preEngagementTask.CurrentTaskNameUrl;
            ViewData["CurrentQuestionNameUrl"] = preEngagementTask.CurrentQuestionNameUrl;
            return View(questionViewModel);
        }

        [HttpPost("tasks")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOrganisationInfo(string currentTaskNameUrl, string currentQuestionNameUrl, [FromForm] IFormCollection formdata)
        {
            QuestionDetails? questionDetails = await _questionService.GetQuestionDetails(currentTaskNameUrl, currentQuestionNameUrl);

            // Convert form data to a dictionary, excluding the anti-forgery token
            var formDataDictionary = FormDataHelper.ConvertToDictionary(formdata);

            // Update the cache with the current task and form data
            UpdateCache(questionDetails.QuestionId, questionDetails.TaskId, formDataDictionary);

            // Redirect to the next task or question
            return RedirectToAction("PreEngagementTaskDetails", new { currentTaskNameUrl, currentQuestionNameUrl });
        }

        [HttpGet("summary")]
        public IActionResult Summary()
        {
            var cacheKey = HttpContext.Session.Id + "_PreEngagementData";
            if (!_memoryCache.TryGetValue<QuestionViewModel>(cacheKey, out var model))
            {
                return RedirectToAction("OrganisationInfo");
            }

            return View("Summary", model);
        }

        public async Task<PreEngagement?> GetPreEngagementTaskDetails(string? currentTaskNameUrl = null, string? currentQuestionNameUrl = null)
        {
            var allPreEngagementTasks = await _taskService.GetPreEngagementTasks();

            if (string.IsNullOrEmpty(currentTaskNameUrl) && string.IsNullOrEmpty(currentQuestionNameUrl))
            {
                return allPreEngagementTasks.FirstOrDefault();
            }

            var currentPreEngagementTask = allPreEngagementTasks.FindIndex
                (p => p.CurrentTaskNameUrl == currentTaskNameUrl && p.CurrentQuestionNameUrl == currentQuestionNameUrl);

            if (currentPreEngagementTask >= 0 && currentPreEngagementTask + 1 < allPreEngagementTasks.Count)
            {
                return allPreEngagementTasks[currentPreEngagementTask + 1];
            }

            return null;
        }

        private void UpdateCache(Guid questionId, Guid taskId, Dictionary<string, string> formDataDictionary)
        {
            var cacheKey = HttpContext.Session.Id + "_PreEngagementData";

            // Retrieve the existing list of Pre Engagement answers from the cache or create a new one if it doesn't exist
            var cachedData = _memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(60);
                return new List<PreEngagementAnswerModel>();
            });

            // Find the existing AnswerModel for the current task
            var existingAnswer = cachedData.FirstOrDefault(a => a.QuestionId == questionId && a.TaskId == taskId);

            // Serialize the form data dictionary to JSON
            var answerJson = JsonSerializer.Serialize(formDataDictionary);

            if (existingAnswer != null)
            {
                // Update existing answer
                existingAnswer.AnswerJson = answerJson;
            }
            else
            {
                // Add new answer
                var answerModel = new PreEngagementAnswerModel
                {
                    QuestionId = questionId,
                    TaskId = taskId,
                    AnswerJson = answerJson
                };
                cachedData.Add(answerModel);
            }
            _memoryCache.Set(cacheKey, cachedData);
        }

        [HttpGet("SeeCache")]
        public IActionResult SeeCache()
        {
            var cacheKey = HttpContext.Session.Id + "_PreEngagementData";
            var cacheData = _memoryCache.Get<List<PreEngagementAnswerModel>>(cacheKey);

            var json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            return Content(json, "application/json");
        }
    }
}
