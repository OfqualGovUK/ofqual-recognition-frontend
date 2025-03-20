
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface ISessionService
{
    public T? GetFromSession<T>(string key) where T : class;
    public void SetInSession<T>(string key, T data) where T : class;
    public bool HasInSession(string key);
    public void ClearFromSession(string key);

    public void UpdateTaskStatusInSession(Guid taskId, TaskStatusEnum newStatus);
}