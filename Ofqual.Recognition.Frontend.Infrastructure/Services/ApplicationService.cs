using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using System.Net.Http.Json;
using Serilog;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IRecognitionCitizenClient _client;
        private readonly ISessionService _sessionService;

        public ApplicationService(IRecognitionCitizenClient client, ISessionService sessionService)
        {
            _client = client;
            _sessionService = sessionService;
        }

        public async Task<Application?> SetUpApplication()
        {
            try
            {
                if (_sessionService.HasInSession(SessionKeys.Application))
                {
                    return _sessionService.GetFromSession<Application>(SessionKeys.Application);
                }

                var client = _client.GetClient();
                var response = await client.PostAsync("/applications", null);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warning("API request to create application failed. Status Code: {StatusCode}, Reason: {Reason}", response.StatusCode, response.ReasonPhrase);
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<Application>();

                if (result != null)
                {
                    _sessionService.SetInSession(SessionKeys.Application, result);
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
}
