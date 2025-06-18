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

            var client = _client.GetClient();
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

            var client = _client.GetClient();
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

    public async Task<ValidationResponse?> ValidatePreEngagementAnswer(Guid questionId, string answerJson)
    {
        try
        {
            var client = _client.GetClient();
            var payload = new QuestionAnswerSubmission { Answer = answerJson };

            var response = await client.PostAsJsonAsync($"/pre-engagement/questions/{questionId}/validate", payload);
            if (response.IsSuccessStatusCode)
            {
                return null;
            }

            ValidationResponse? validationResponse = await response.Content.ReadFromJsonAsync<ValidationResponse>();
            if (validationResponse == null)
            {
                Log.Warning("Pre-engagement validation response was null. QuestionId: {QuestionId}, StatusCode: {StatusCode}", questionId, response.StatusCode);
                return null;
            }

            return validationResponse;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while validating the pre-engagement answer for QuestionId: {QuestionId}", questionId);
            return null;
        }
    }
}