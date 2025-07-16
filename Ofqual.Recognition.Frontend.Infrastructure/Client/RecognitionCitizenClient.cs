using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

using System.Net.Http.Headers;

using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Serilog;

namespace Ofqual.Recognition.Frontend.Infrastructure.Client;

public class RecognitionCitizenClient : IRecognitionCitizenClient
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;

    public RecognitionCitizenClient(IHttpClientFactory clientFactory, ITokenAcquisition tokenAcquisition, IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;
    }

    public async Task<HttpClient> GetClientAsync(bool withAccessToken = true)
    {
        var scopes = _configuration
            .GetSection("RecognitionApi:Scopes")
            .Get<IEnumerable<string>>();

        if (scopes == null || !scopes.Any())
        {
            throw new InvalidOperationException("RecognitionApi:Scopes configuration is missing or empty.");
        }

        var client = _clientFactory.CreateClient("RecognitionCitizen");

        if (true)
        {
            try
            {
                var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                Log.Debug(ex, "User not authenticated, skipping access token");
                throw;
                // IMPORTANT: You must throw this error all the way through the controller for this to be handled appropriately if using this client to access a protected resource.
                // Failure to do so will cause authentication issues for the end user when the application restarts!
            }
        }

        return client;
    }
}
