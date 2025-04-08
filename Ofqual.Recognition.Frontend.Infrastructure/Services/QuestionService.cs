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

    public async Task<QuestionAnswerResult?> SubmitQuestionAnswer(Guid applicationId, Guid questionId, string answer)
    {
        try
        {
            var client = _client.GetClient();
            var payload = new QuestionAnswer
            {
                Answer = answer
            };

            var response = await client.PostAsJsonAsync($"/applications/{applicationId}/questions/{questionId}", payload);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to submit answer for question {QuestionId} in application {ApplicationId}", questionId, applicationId);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<QuestionAnswerResult>();
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while submitting answer for question {QuestionId} in application {ApplicationId}", questionId, applicationId);
            return null;
        }
    }
}