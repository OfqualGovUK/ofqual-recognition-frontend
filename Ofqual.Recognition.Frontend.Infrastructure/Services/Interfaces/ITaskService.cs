﻿using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface ITaskService
{
    Task<List<TaskItemStatusSection>> GetApplicationTasks(Guid applicationId);
    Task<bool> UpdateTaskStatus(Guid applicationId, Guid taskId, TaskStatusEnum status);
}
