using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Serilog;
using Microsoft.Identity.Web;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IRecognitionCitizenClient _client;
    private readonly IMemoryCacheService _memoryCacheService;

    public AttachmentService(IRecognitionCitizenClient client, IMemoryCacheService memoryCacheService)
    {
        _client = client;
        _memoryCacheService = memoryCacheService;
    }

    public async Task<AttachmentDetails?> UploadFileToLinkedRecord(LinkType linkType, Guid linkId, Guid applicationId, IFormFile file)
    {
        try
        {
            var cacheKey = $"{MemoryCacheKeys.UploadedFilesByQuestion}:{linkType}:{linkId}:{applicationId}";
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
                Log.Warning("File upload failed. LinkType: {LinkType}, LinkId: {LinkId}, AppId: {AppId}, StatusCode: {StatusCode}, Reason: {Reason}", linkType, linkId, applicationId, response.StatusCode, response.ReasonPhrase);
                return null;
            }

            var uploadedAttachment = await response.Content.ReadFromJsonAsync<AttachmentDetails>();
            if (uploadedAttachment != null)
            {
                _memoryCacheService.AddOrAppendToList(cacheKey, uploadedAttachment);
            }

            return uploadedAttachment;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            Log.Debug(ex, "User not authenticated, cannot upload file.");
            throw; // Re-throw to handle authentication challenge
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An exception occurred while uploading file. LinkType: {LinkType}, LinkId: {LinkId}, AppId: {AppId}", linkType, linkId, applicationId);
            return null;
        }
    }

    public async Task<List<AttachmentDetails>> GetAllLinkedFiles(LinkType linkType, Guid linkId, Guid applicationId)
    {
        try
        {
            var memoryCacheKey = $"{MemoryCacheKeys.UploadedFilesByQuestion}:{linkType}:{linkId}:{applicationId}";
            if (_memoryCacheService.HasInMemoryCache(memoryCacheKey))
            {
                return _memoryCacheService.Get<List<AttachmentDetails>>(memoryCacheKey) ?? new List<AttachmentDetails>();
            }

            var client = await _client.GetClientAsync();

            var response = await client.GetAsync($"/files/linked/{linkType}/{linkId}/application/{applicationId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("No linked files returned for LinkType: {LinkType}, LinkId: {LinkId}, ApplicationId: {AppId}", linkType, linkId, applicationId);
                return new List<AttachmentDetails>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<AttachmentDetails>>() ?? new List<AttachmentDetails>();
            _memoryCacheService.Set(memoryCacheKey, result);
            return result;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            Log.Debug(ex, "User not authenticated, cannot retrieve linked files.");
            throw; // Re-throw to handle authentication challenge
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while retrieving linked files for LinkType: {LinkType}, LinkId: {LinkId}, ApplicationId: {AppId}", linkType, linkId, applicationId);
            return new List<AttachmentDetails>();
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
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            Log.Debug(ex, "User not authenticated, cannot download file.");
            throw; // Re-throw to handle authentication challenge
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
            var cacheKey = $"{MemoryCacheKeys.UploadedFilesByQuestion}:{linkType}:{linkId}:{applicationId}";
            var client = await _client.GetClientAsync();

            var response = await client.DeleteAsync($"/files/linked/{linkType}/{linkId}/attachment/{attachmentId}/application/{applicationId}");
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to delete file. LinkType: {LinkType}, LinkId: {LinkId}, AttachmentId: {AttachmentId}, AppId: {AppId}, StatusCode: {StatusCode}, Reason: {Reason}", linkType, linkId, attachmentId, applicationId, response.StatusCode, response.ReasonPhrase);
                return false;
            }

            _memoryCacheService.RemoveFromList<AttachmentDetails>(cacheKey, a => a.AttachmentId == attachmentId);
            return true;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            Log.Debug(ex, "User not authenticated, cannot delete file.");
            throw; // Re-throw to handle authentication challenge
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An exception occurred while deleting file. LinkType: {LinkType}, LinkId: {LinkId}, AttachmentId: {AttachmentId}, AppId: {AppId}", linkType, linkId, attachmentId, applicationId);
            return false;
        }
    }
}