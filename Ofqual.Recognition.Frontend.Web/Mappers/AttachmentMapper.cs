using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public static class AttachmentMapper
{
    public static List<AttachmentDetailsViewModel> MapToViewModel(List<AttachmentDetails> attachments)
    {
        return attachments.Select(a => new AttachmentDetailsViewModel
        {
            AttachmentId = a.AttachmentId,
            FileName = a.FileName,
            FileMIMEtype = a.FileMIMEtype,
            FileSize = a.FileSize,
            IsInOtherCriteria = a.IsInOtherCriteria
        }).ToList();
    }
}
