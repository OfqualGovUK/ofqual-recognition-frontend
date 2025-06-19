using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
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
    private readonly ISessionService _sessionService;

    public AttachmentService(IRecognitionCitizenClient client, ISessionService sessionService)
    {
        _client = client;
        _sessionService = sessionService;
    }

    public async Task<AttachmentDetails?> UploadFileToLinkedRecord(LinkType linkType, Guid linkId, Guid applicationId, IFormFile file)
    {
        try
        {
            var client = await _client.GetClientAsync();

            using var content = new MultipartFormDataContent();

            if (file != null && file.Length > 0)
            {
                using var fileStream = file.OpenReadStream();
                var fileContent = new StreamContent(fileStream)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
                };
                content.Add(fileContent, "file", file.FileName);
            }

            var response = await client.PostAsync($"/files/linked/{linkType}/{linkId}/application/{applicationId}", content);
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("File upload failed. LinkType: {LinkType}, LinkId: {LinkId}, AppId: {AppId}, StatusCode: {StatusCode}, Reason: {Reason}", linkType, linkId, applicationId, response.StatusCode, response.ReasonPhrase);
                return null;
            }

            _sessionService.ClearFromSession($"{SessionKeys.UploadedFiles}/{linkType}/{linkId}");
            return await response.Content.ReadFromJsonAsync<AttachmentDetails>();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An exception occurred while uploading file. LinkType: {LinkType}, LinkId: {LinkId}, AppId: {AppId}", linkType, linkId, applicationId);
            return null;
        }
    }

    public async Task<List<AttachmentDetails>?> GetAllLinkedFiles(LinkType linkType, Guid linkId, Guid applicationId)
    {
        try
        {
            var sessionKey = $"{SessionKeys.UploadedFiles}/{linkType}/{linkId}";

            if (_sessionService.HasInSession(sessionKey))
            {
                return _sessionService.GetFromSession<List<AttachmentDetails>>(sessionKey);
            }

            var client = await _client.GetClientAsync();

            var result = await client.GetFromJsonAsync<List<AttachmentDetails>>($"/files/linked/{linkType}/{linkId}/application/{applicationId}");
            if (result == null || result.Count == 0)
            {
                Log.Warning("No linked files returned for LinkType: {LinkType}, LinkId: {LinkId}, ApplicationId: {AppId}", linkType, linkId, applicationId);
                result = new List<AttachmentDetails>();
            }

            _sessionService.SetInSession(sessionKey, result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving linked files for LinkType: {LinkType}, LinkId: {LinkId}, ApplicationId: {AppId}", linkType, linkId, applicationId);
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
                Log.Warning("File download failed. LinkType: {LinkType}, LinkId: {LinkId}, AttachmentId: {AttachmentId}, AppId: {AppId}, StatusCode: {StatusCode}, Reason: {Reason}", linkType, linkId, attachmentId, applicationId, response.StatusCode, response.ReasonPhrase);
                return null;
            }

            return await response.Content.ReadAsStreamAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An exception occurred while downloading file. LinkType: {LinkType}, LinkId: {LinkId}, AttachmentId: {AttachmentId}, AppId: {AppId}", linkType, linkId, attachmentId, applicationId);
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
                Log.Warning("Failed to delete file. LinkType: {LinkType}, LinkId: {LinkId}, AttachmentId: {AttachmentId}, AppId: {AppId}, StatusCode: {StatusCode}, Reason: {Reason}", linkType, linkId, attachmentId, applicationId, response.StatusCode, response.ReasonPhrase);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An exception occurred while deleting file. LinkType: {LinkType}, LinkId: {LinkId}, AttachmentId: {AttachmentId}, AppId: {AppId}", linkType, linkId, attachmentId, applicationId);
            return false;
        }
    }
}