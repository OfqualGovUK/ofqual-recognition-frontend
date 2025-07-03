using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface ITaskService
{
    public Task<List<TaskItemStatusSection>> GetApplicationTasks(Guid applicationId);
    public Task<bool> UpdateTaskStatus(Guid applicationId, Guid taskId, StatusType status);
    public Task<TaskDetails?> GetTaskDetailsByTaskNameUrl(string taskNameUrl);
}
