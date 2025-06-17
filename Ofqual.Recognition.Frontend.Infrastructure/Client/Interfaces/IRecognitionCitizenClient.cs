namespace Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;

public interface IRecognitionCitizenClient
{
    Task<HttpClient> GetClientAsync();
}
