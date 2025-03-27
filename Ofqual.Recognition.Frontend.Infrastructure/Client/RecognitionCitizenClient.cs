using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Client;

public class RecognitionCitizenClient : IRecognitionCitizenClient
{
    private readonly IHttpClientFactory _clientFactory;

    public RecognitionCitizenClient(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public HttpClient GetClient()
    {
        return _clientFactory.CreateClient("RecognitionCitizen");
    }
}
