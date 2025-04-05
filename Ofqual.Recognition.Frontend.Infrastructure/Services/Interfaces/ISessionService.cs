using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface ISessionService
{
    T? GetFromSession<T>(string key) where T : class;
    void SetInSession<T>(string key, T data) where T : class;
    bool HasInSession(string key);
    void ClearFromSession(string key);
    void UpdateTaskStatusInSession(Guid taskId, TaskStatusEnum newStatus);
}