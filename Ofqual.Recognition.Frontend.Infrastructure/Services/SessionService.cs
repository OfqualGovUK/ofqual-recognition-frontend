using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets a stored object from session.
    /// </summary>
    public T? GetFromSession<T>(string key) where T : class
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return null;

        if (!session.TryGetValue(key, out var data)) return null;

        var jsonData = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(jsonData);
    }

    /// <summary>
    /// Stores an object in session.
    /// </summary>
    public void SetInSession<T>(string key, T data) where T : class
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return;
        var jsonData = JsonConvert.SerializeObject(data);
        session.SetString(key, jsonData);
    }

    /// <summary>
    /// Checks if a key exists in session.
    /// </summary>
    public bool HasInSession(string key)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return false;

        return session.TryGetValue(key, out var value) && value?.Length > 0;
    }

    /// <summary>
    /// Removes an object from session.
    /// </summary>
    public void ClearFromSession(string key)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove(key);
    }

    /// <summary>
    /// Updates the status of a specific task in the session.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="newStatus">The new status for the task.</param>
    public void UpdateTaskStatusInSession(Guid taskId, TaskStatusEnum newStatus)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return;
        
        if (!session.TryGetValue(SessionKeys.TaskList, out var data)) return;
        var serializedTasks = Encoding.UTF8.GetString(data);
        if (string.IsNullOrEmpty(serializedTasks)) return;

        var taskSections = JsonConvert.DeserializeObject<List<TaskItemStatusSection>>(serializedTasks);
        if (taskSections == null) return;

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
        session.Set(SessionKeys.TaskList, Encoding.UTF8.GetBytes(updatedData));
    }
}


