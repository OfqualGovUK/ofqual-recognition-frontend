using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Serilog;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IRecognitionCitizenClient _client;

    public AttachmentService(IRecognitionCitizenClient client)
    {
        _client = client;
    }

    public async Task<AttachmentDetails?> UploadFileToLinkedRecord(LinkType linkType, Guid linkId, Guid applicationId, IFormFile file)
    {
        try
        {
            var client = await _client.GetClientAsync();

            using var content = new MultipartFormDataContent();
            if (file != null && file.Length > 0)
            {
                var fileStream = file.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);
            }

            var response = await client.PostAsync($"/files/linked/{linkType}/{linkId}/application/{applicationId}", content);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("File upload failed. Status Code: {StatusCode}, Reason: {Reason}", response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<AttachmentDetails>();
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unexpected error occurred while uploading the file.");
            return null;
        }
    }

    public async Task<List<AttachmentDetails>?> GetAllLinkedFiles(LinkType linkType, Guid linkId, Guid applicationId)
    {
        try
        {
            var client = await _client.GetClientAsync();

            var response = await client.GetAsync($"/files/linked/{linkType}/{linkId}/application/{applicationId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to retrieve files. Status Code: {StatusCode}, Reason: {Reason}", response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var files = await response.Content.ReadFromJsonAsync<List<AttachmentDetails>>();
            return files;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while fetching linked files.");
            return null;
        }
    }

    public async Task<Stream?> DownloadLinkedFile(LinkType linkType, Guid linkId, Guid attachmentId, Guid applicationId)
    {
        try
        {
            var client = await _client.GetClientAsync();

            var response = await client.GetAsync($"/files/linked/{linkType}/{linkId}/attachment/{attachmentId}/application/{applicationId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("File download failed. Status Code: {StatusCode}, Reason: {Reason}", response.StatusCode, response.ReasonPhrase);
                return null;
            }

            return await response.Content.ReadAsStreamAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while downloading the file.");
            return null;
        }
    }

    public async Task<bool> DeleteLinkedFile(LinkType linkType, Guid linkId, Guid attachmentId, Guid applicationId)
    {
        try
        {
            var client = await _client.GetClientAsync();

            var response = await client.DeleteAsync($"/files/linked/{linkType}/{linkId}/attachment/{attachmentId}/application/{applicationId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to delete file. Status Code: {StatusCode}, Reason: {Reason}", response.StatusCode, response.ReasonPhrase);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while deleting the file.");
            return false;
        }
    }
}