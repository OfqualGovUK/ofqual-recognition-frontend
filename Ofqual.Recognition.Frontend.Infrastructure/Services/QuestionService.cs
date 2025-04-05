using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Serilog;
using System.Net.Http.Json;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly IRecognitionCitizenClient _client;
    private readonly ISessionService _sessionService;

    public QuestionService(IRecognitionCitizenClient recognitionCitizenClient, ISessionService sessionService)
    {
        _client = recognitionCitizenClient;
        _sessionService = sessionService;
    }

    public async Task<QuestionResponse?> GetQuestionDetails(string taskName, string questionName)
    {
        try
        {
            if (_sessionService.HasInSession($"{taskName}/{questionName}"))
            {
                return _sessionService.GetFromSession<QuestionResponse>($"{taskName}/{questionName}");
            }

            var client = _client.GetClient();
            var result = await client.GetFromJsonAsync<QuestionResponse>($"/questions/{taskName}/{questionName}");

            if (result == null)
            {
                Log.Warning("No question found with URL: {taskName}/{questionName}", taskName, questionName);
                return result;
            }

            _sessionService.SetInSession($"{taskName}/{questionName}", result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving question for URL: {taskName}/{questionName}", taskName, questionName);
            return null;
        }
    }
}
