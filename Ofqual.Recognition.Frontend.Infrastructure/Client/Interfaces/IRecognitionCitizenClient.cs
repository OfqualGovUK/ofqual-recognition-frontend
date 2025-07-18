namespace Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;

public interface IRecognitionCitizenClient
{
    public Task<HttpClient> GetClientAsync(bool withAccessToken = true);
}
