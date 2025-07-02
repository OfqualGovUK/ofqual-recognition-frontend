using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Helpers;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public T? GetFromSession<T>(string key) where T : class
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return null;
        }

        if (!session.TryGetValue(key, out var data))
        {
            return null;
        }

        var jsonData = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(jsonData);
    }

    public void SetInSession<T>(string key, T data) where T : class
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return;
        }

        var jsonData = JsonConvert.SerializeObject(data);
        session.SetString(key, jsonData);
    }

    public bool HasInSession(string key)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return false;
        }

        return session.TryGetValue(key, out var value) && value?.Length > 0;
    }

    public void ClearFromSession(string key)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove(key);
    }

    public void ClearAllSession()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Clear();
    }

    public void UpdateTaskStatusInSession(Guid taskId, StatusType newStatus)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return;
        }

        if (!session.TryGetValue(SessionKeys.ApplicationTaskList, out var data))
        {
            return;
        }

        var serializedTasks = Encoding.UTF8.GetString(data);
        if (string.IsNullOrEmpty(serializedTasks))
        {
            return;
        }

        var taskSections = JsonConvert.DeserializeObject<List<TaskItemStatusSection>>(serializedTasks);
        if (taskSections == null)
        {
            return;
        }

        foreach (var section in taskSections)
        {
            var task = section.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                task.Status = newStatus;
                break;
            }
        }

        var updatedData = JsonConvert.SerializeObject(taskSections);
        session.Set(SessionKeys.ApplicationTaskList, Encoding.UTF8.GetBytes(updatedData));
    }

    public StatusType? GetTaskStatusFromSession(Guid taskId)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return null;
        }

        if (!session.TryGetValue(SessionKeys.ApplicationTaskList, out var data))
        {
            return null;
        }

        var serializedTasks = Encoding.UTF8.GetString(data);
        if (string.IsNullOrEmpty(serializedTasks))
        {
            return null;
        }

        var taskSections = JsonConvert.DeserializeObject<List<TaskItemStatusSection>>(serializedTasks);
        if (taskSections == null)
        {
            return null;
        }

        foreach (var section in taskSections)
        {
            var task = section.Tasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
            {
                return task.Status;
            }
        }

        return null;
    }

    public void UpsertPreEngagementAnswer(Guid questionId, Guid taskId, string answerJson)
    {
        if (string.IsNullOrWhiteSpace(answerJson))
        {
            throw new ArgumentException("Answer JSON cannot be null or empty.", nameof(answerJson));
        }

        if (JsonHelper.IsEmptyJsonObject(answerJson))
        {
            return;
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            return;
        }

        const string sessionKey = SessionKeys.PreEngagementAnswers;

        List<PreEngagementAnswer> cachedAnswers;
        if (session.TryGetValue(sessionKey, out var data))
        {
            var json = Encoding.UTF8.GetString(data);
            cachedAnswers = JsonConvert.DeserializeObject<List<PreEngagementAnswer>>(json) ?? new List<PreEngagementAnswer>();
        }
        else
        {
            cachedAnswers = new List<PreEngagementAnswer>();
        }

        var existing = cachedAnswers.FirstOrDefault(a => a.QuestionId == questionId && a.TaskId == taskId);

        if (existing != null)
        {
            if (JsonHelper.AreEqual(existing.AnswerJson, answerJson))
            {
                return;
            }
            existing.AnswerJson = answerJson;
        }
        else
        {
            cachedAnswers.Add(new PreEngagementAnswer
            {
                QuestionId = questionId,
                TaskId = taskId,
                AnswerJson = answerJson
            });
        }

        var updatedJson = JsonConvert.SerializeObject(cachedAnswers);
        session.Set(sessionKey, Encoding.UTF8.GetBytes(updatedJson));
    }
}