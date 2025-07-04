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

    public async Task<HttpClient> GetClientAsync()
    {
        var scopes = _configuration
            .GetSection("RecognitionApi:Scopes")
            .Get<IEnumerable<string>>();

        if (scopes == null || !scopes.Any())
        {
            throw new InvalidOperationException("RecognitionApi:Scopes configuration is missing or empty.");
        }

        var client = _clientFactory.CreateClient("RecognitionCitizen");

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
        }

        return client;
    }
}
