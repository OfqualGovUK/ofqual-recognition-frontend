namespace Ofqual.Recognition.Frontend.Infrastructure.Client
{
    public class RecognitionCitizenClient
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
}
