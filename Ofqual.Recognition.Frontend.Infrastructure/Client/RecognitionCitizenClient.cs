using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using System.Net.Http.Headers;

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

    public async Task<HttpClient> GetClient()
    {
        var client = _clientFactory.CreateClient("RecognitionCitizen");
        var scopes = _configuration.GetSection("DownstreamApis:CitizenAPI:Scopes").Get<IEnumerable<string>>();

        if (scopes == null || !scopes.Any())
        {
            throw new InvalidOperationException("No scopes configured for DownstreamApis:CitizenAPI:Scopes.");
        }

        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return _clientFactory.CreateClient("RecognitionCitizen");
    }
}
