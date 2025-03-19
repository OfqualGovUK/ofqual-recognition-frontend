namespace Ofqual.Recognition.Frontend.Core.Models
{
    public class ApplicationModel
    {
        public Guid ApplicationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public required string CreatedByUpn { get; set; }
        public string? ModifiedByUpn { get; set; }
    }
}
