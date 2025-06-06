namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class AttachmentDetailsViewModel
{
    public Guid AttachmentId { get; set; }
    public required string FileName { get; set; }
    public required string FileMIMEtype { get; set; }
    public long FileSize { get; set; }

    public bool HasSent { get; set; }
}