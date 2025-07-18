using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Identity.Web;

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
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            Log.Debug(ex, "User not authenticated, cannot initialise application.");
            throw; // Re-throw to handle authentication challenge
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while initialising the application.");
            return null;
        }
    }

    public async Task<Application?> GetLatestApplication()
    {
        var applicationSessionKey = SessionKeys.Application;
        if (_sessionService.HasInSession(applicationSessionKey))
        {
            return _sessionService.GetFromSession<Application>(applicationSessionKey);
        }

        var client = await _client.GetClientAsync();

        var response = await client.GetAsync("/applications");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This indicates that we legitimately do not have an application in session or API, which is expected if the user has not started an application yet.
                // Hence why in this case, it is safe to return a null
                Log.Warning("No application found in session or API, but was otherwise successful.");
                return null;
            }
            else
            {
                // IMPORTANT: Any type of error where we cannot find an app due to a technical issue must throw an exception all the way up and out the controller
                Log.Error("Exception raised when attempting to contact API to get Application Data, in ApplicationService::GetLatestApplication. Exception message: {response.ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new ApplicationException("Exception raised when attempting to contact API to get Application Data, in ApplicationService::GetLatestApplication. Exception message: " + response.ReasonPhrase);
            }
        }

        var result = await response.Content.ReadFromJsonAsync<Application>();

        if (result != null)
        {
            _sessionService.SetInSession(applicationSessionKey, result);
        }
        else
        {
            Log.Warning("In ApplicationService::GetLatestApplication, a success status code was received but contained no Application Data");
        }
        return result;
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
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            Log.Debug(ex, "User not authenticated, cannot submit application.");
            throw; // Re-throw to handle authentication challenge
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error submitting Application {ApplicationId}", applicationId);
            return null;
        }
    }
}