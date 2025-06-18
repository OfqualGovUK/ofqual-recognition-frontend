namespace Ofqual.Recognition.Frontend.Core.Models;

public class ValidationResponse
{
    public IEnumerable<ValidationErrorItem>? Errors { get; set; }
}