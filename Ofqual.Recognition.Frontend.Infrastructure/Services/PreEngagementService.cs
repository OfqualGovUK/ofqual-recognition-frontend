using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using System.Net.Http.Json;
using Serilog;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class PreEngagementService : IPreEngagementService
{
    private readonly IRecognitionCitizenClient _client;
    private readonly ISessionService _sessionService;

    public PreEngagementService(IRecognitionCitizenClient client, ISessionService sessionService)
    {
        _client = client;
        _sessionService = sessionService;
    }

    public async Task<PreEngagementQuestion?> GetFirstPreEngagementQuestion()
    {
        try
        {
            var sessionKey = SessionKeys.FirstPreEngagementQuestion;

            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<PreEngagementQuestion>(sessionKey);
            }

            var client = await _client.GetClientAsync();
            var result = await client.GetFromJsonAsync<PreEngagementQuestion>("/pre-engagement/first-question");

            if (result == null)
            {
                Log.Warning("No first pre-engagement question found.");
                return null;
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred whilst retrieving the first pre-engagement question.");
            return null;
        }
    }

    public async Task<QuestionDetails?> GetPreEngagementQuestionDetails(string taskNameUrl, string questionNameUrl)
    {
        try
        {
            var sessionKey = $"{SessionKeys.PreEngagementQuestionDetails}/{taskNameUrl}/{questionNameUrl}";

            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<QuestionDetails>(sessionKey);
            }

            var client = await _client.GetClientAsync();
            var result = await client.GetFromJsonAsync<QuestionDetails>($"/pre-engagement/{taskNameUrl}/{questionNameUrl}");

            if (result == null)
            {
                Log.Warning("No pre-engagement question found for URL: {taskName}/{questionName}", taskNameUrl, questionNameUrl);
                return result;
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred whilst retrieving pre-engagement question for URL: {taskName}/{questionName}", taskNameUrl, questionNameUrl);
            return null;
        }
    }
}