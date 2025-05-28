using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using System.Net.Http.Json;
using Serilog;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class ApplicationService : IApplicationService
{
    private readonly IRecognitionCitizenClient _client;
    private readonly ISessionService _sessionService;
    private readonly IMemoryCacheService _memoryCacheService;

    public ApplicationService(IRecognitionCitizenClient client, ISessionService sessionService, IMemoryCacheService memoryCacheService)
    {
        _client = client;
        _sessionService = sessionService;
        _memoryCacheService = memoryCacheService;
    }

    public async Task<Application?> SetUpApplication()
    {
        try
        {
            var sessionKey = SessionKeys.Application;
            var preEngagementAnswersKey = SessionKeys.PreEngagementAnswers;

            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<Application>(sessionKey);
            }

            var preEngagementAnswers = _memoryCacheService.GetFromCache<List<PreEngagementAnswer>>(preEngagementAnswersKey);

            var client = _client.GetClient();
            var response = await client.PostAsJsonAsync("/applications", preEngagementAnswers);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("API request to create application failed. Status Code: {StatusCode}, Reason: {Reason}", response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<Application>();

            if (result != null)
            {
                _sessionService.SetInSession(sessionKey, result);
                _memoryCacheService.RemoveFromCache(preEngagementAnswersKey);
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while setting up the application.");
            return null;
        }
    }
}
