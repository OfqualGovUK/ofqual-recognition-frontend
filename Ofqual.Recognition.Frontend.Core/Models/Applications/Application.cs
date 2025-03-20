namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents a domain-level model of the <c>recognitionCitizen.Application</c> database table,
/// defining applications and their audit tracking information.
/// </summary>
public class Application
{
    public Guid ApplicationId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public required string CreatedByUpn  { get; set; }
    public string? ModifiedByUpn  { get; set; }
}