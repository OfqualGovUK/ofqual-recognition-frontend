
using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface ISessionService
{
    public Application? GetApplication();
    public void SetApplication(Application application);
    bool HasApplication();

    List<TaskItemStatusSection> GetTasks();
    public void SetTasks(List<TaskItemStatusSection> tasks);
    public bool HasTasks();
    public void ClearTasks();
}