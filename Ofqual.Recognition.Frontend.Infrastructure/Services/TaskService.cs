using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Text;
using Serilog;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly IRecognitionCitizenClient _client;
    private readonly ISessionService _sessionService;

    public TaskService(IRecognitionCitizenClient client, ISessionService sessionService)
    {
        _client = client;
        _sessionService = sessionService;
    }

    public async Task<List<TaskItemStatusSection>> GetApplicationTasks(Guid applicationId)
    {
        try
        {
            var sessionKey = SessionKeys.ApplicationTaskList;
            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<List<TaskItemStatusSection>>(sessionKey) ?? new List<TaskItemStatusSection>();
            }

            var client = await _client.GetClientAsync();

            var result = await client.GetFromJsonAsync<List<TaskItemStatusSection>>($"/applications/{applicationId}/tasks");
            if (result == null || result.Count == 0)
            {
                Log.Warning("No tasks found for Application ID {ApplicationId}", applicationId);
                result = new List<TaskItemStatusSection>();
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving tasks for Application ID {ApplicationId}", applicationId);
            return new List<TaskItemStatusSection>();
        }
    }

    public async Task<Application?> UpdateTaskStatus(Guid applicationId, Guid taskId, StatusType status)
    {
        try
        {
            StatusType? currentStatus = _sessionService.GetTaskStatusFromSession(taskId);
            if (currentStatus == status)
            {
                return _sessionService.GetFromSession<Application>(SessionKeys.Application);
            }

            var client = await _client.GetClientAsync();
            var content = new StringContent(
                JsonConvert.SerializeObject(new UpdateTaskStatus { Status = status }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"/applications/{applicationId}/tasks/{taskId}", content);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to update task {TaskId} for Application {ApplicationId}. StatusCode: {StatusCode}, Reason: {ReasonPhrase}", taskId, applicationId, response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var application = JsonConvert.DeserializeObject<Application>(json);

            if (application != null)
            {
                _sessionService.UpdateTaskStatusInSession(taskId, status);
                _sessionService.SetInSession(SessionKeys.Application, application);
            }

            return application;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating task {TaskId} for Application {ApplicationId}", taskId, applicationId);
            return null;
        }
    }

    public async Task<TaskDetails?> GetTaskDetailsByTaskNameUrl(string taskNameUrl)
    {
        try
        {
            var sessionKey = $"{SessionKeys.ApplicationTaskDetails}:{taskNameUrl}";
            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<TaskDetails>(sessionKey);
            }

            var client = await _client.GetClientAsync();
            var result = await client.GetFromJsonAsync<TaskDetails>($"/tasks/{taskNameUrl}");

            if (result == null)
            {
                Log.Warning("No task details found for TaskNameUrl '{TaskNameUrl}'", taskNameUrl);
                return result;
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving task details for TaskNameUrl '{TaskNameUrl}'", taskNameUrl);
            return null;
        }
    }
}
