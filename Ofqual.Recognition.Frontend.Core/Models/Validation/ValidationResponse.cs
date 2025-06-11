namespace Ofqual.Recognition.Frontend.Core.Models;

public class ValidationResponse
{
    public string? Message { get; set; }
    public IEnumerable<ValidationErrorItem>? Errors { get; set; }
}
