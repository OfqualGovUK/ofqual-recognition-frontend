using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;

using System.Net.Http.Headers;

using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;


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
        var scopes = _configuration.GetSection("RecognitionApi:Scopes").Get<IEnumerable<string>>();

        var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

        HttpClient client = _clientFactory
            .CreateClient("RecognitionCitizen");

        if (accessToken != null)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
        
        return client;
    }
}
