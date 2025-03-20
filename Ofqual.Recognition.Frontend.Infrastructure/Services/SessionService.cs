
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Application? GetApplication()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return null;
        var appJson = session.GetString("Application");
        return appJson != null ? JsonConvert.DeserializeObject<Application>(appJson) : null;
    }

    public void SetApplication(Application application)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.SetString("Application", JsonConvert.SerializeObject(application));
    }

    public bool HasApplication()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return session?.GetString("Application") != null;
    }

    public List<TaskItemStatusSectionDto> GetTasks()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        var serializedTasks = session?.GetString("TaskList");
        return string.IsNullOrEmpty(serializedTasks)
            ? new List<TaskItemStatusSectionDto>()
            : JsonConvert.DeserializeObject<List<TaskItemStatusSectionDto>>(serializedTasks) ?? new List<TaskItemStatusSectionDto>();
    }

    public void SetTasks(List<TaskItemStatusSectionDto> tasks)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null) return;
        var serializedTasks = JsonConvert.SerializeObject(tasks);
        session.SetString("TaskList", serializedTasks);
    }

    public bool HasTasks()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return session?.GetString("TaskList") != null;
    }

    public void ClearTasks()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        session?.Remove("TaskList");
    }
}


