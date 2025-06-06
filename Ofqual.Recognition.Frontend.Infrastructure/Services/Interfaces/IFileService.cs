using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IFileService
{
    public Task<AttachmentDetails?> UploadFileToLinkedRecord(LinkType linkType, Guid linkId, Guid applicationId, IFormFile file);
    public Task<List<AttachmentDetails>?> GetAllLinkedFiles(LinkType linkType, Guid linkId, Guid applicationId);
    public Task<Stream?> DownloadLinkedFile(LinkType linkType, Guid linkId, Guid attachmentId, Guid applicationId);
    public Task<bool> DeleteLinkedFile(LinkType linkType, Guid linkId, Guid attachmentId, Guid applicationId);
}
