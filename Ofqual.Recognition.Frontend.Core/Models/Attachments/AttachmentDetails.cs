namespace Ofqual.Recognition.Frontend.Core.Models;

public class AttachmentDetails
{
    public Guid AttachmentId { get; set; }
    public required string FileName { get; set; }
    public required string FileMIMEtype { get; set; }
    public long FileSize { get; set; }
}