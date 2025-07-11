﻿using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Serilog;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class ApplicationService : IApplicationService
{
    private readonly IRecognitionCitizenClient _client;
    private readonly ISessionService _sessionService;

    public ApplicationService(IRecognitionCitizenClient client, ISessionService sessionService)
    {
        _client = client;
        _sessionService = sessionService;
    }

    public async Task<Application?> InitialiseApplication()
    {
        try
        {
            var applicationSessionKey = SessionKeys.Application;
            var preEngagementAnswersSessionKey = SessionKeys.PreEngagementAnswers;

            if (_sessionService.HasInSession(applicationSessionKey))
            {
                return _sessionService.GetFromSession<Application>(applicationSessionKey);
            }

            var preEngagementAnswers = _sessionService.GetFromSession<List<PreEngagementAnswer>>(preEngagementAnswersSessionKey);

            var client = await _client.GetClientAsync();
            var response = await client.PostAsJsonAsync("/applications", preEngagementAnswers);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("API request to initialise application failed. Status Code: {StatusCode}, Reason: {Reason}", response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<Application>();

            if (result != null)
            {
                _sessionService.SetInSession(applicationSessionKey, result);
                _sessionService.ClearFromSession(preEngagementAnswersSessionKey);
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while initialising the application.");
            return null;
        }
    }

    public async Task<Application?> SubmitApplication(Guid applicationId)
    {
        try
        {
            var client = await _client.GetClientAsync();

            var response = await client.PostAsync($"/applications/{applicationId}/submit", null);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to submit Application {ApplicationId}. StatusCode: {StatusCode}, Reason: {ReasonPhrase}", applicationId, response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var application = JsonConvert.DeserializeObject<Application>(json);

            if (application != null)
            {
                _sessionService.SetInSession(SessionKeys.Application, application);
            }

            return application;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error submitting Application {ApplicationId}", applicationId);
            return null;
        }
    }
}