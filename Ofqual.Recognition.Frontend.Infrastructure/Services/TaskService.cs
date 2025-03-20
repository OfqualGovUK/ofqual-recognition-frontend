using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Models;
using System.Net.Http.Json;
using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Serilog;
using Newtonsoft.Json;
using System.Text;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services
{
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
                if (_sessionService.HasTasks())
                {
                    return _sessionService.GetTasks();
                }

                var client = _client.GetClient();
                var result = await client.GetFromJsonAsync<List<TaskItemStatusSection>>($"/applications/{applicationId}/tasks");

                if (result == null || result.Count == 0)
                {
                    Log.Warning("No tasks found for Application ID {ApplicationId}", applicationId);
                    result = new List<TaskItemStatusSection>();
                }

                _sessionService.SetTasks(result);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving tasks for Application ID {ApplicationId}", applicationId);
                return new List<TaskItemStatusSection>();
            }
        }

        public async Task<bool> UpdateTaskStatus(Guid applicationId, Guid taskId, TaskStatusEnum status)
        {
            try
            {
                var client = _client.GetClient();
                var jsonContent = new StringContent(JsonConvert.SerializeObject(status), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"/applications/{applicationId}/tasks/{taskId}", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("Failed to update task {TaskId} for Application ID {ApplicationId}. Status Code: {StatusCode}, Reason: {Reason}", taskId, applicationId, response.StatusCode, response.ReasonPhrase);
                    return false;
                }

                _sessionService.ClearTasks();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating task {TaskId} for Application ID {ApplicationId}", taskId, applicationId);
                return false;
            }
        }
    }
}