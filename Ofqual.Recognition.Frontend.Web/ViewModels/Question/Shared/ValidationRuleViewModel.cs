namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class ValidationRuleViewModel
{
    /// <summary>
    /// The maximum length allowed.
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// The minimum length required.
    /// </summary>
    public int? MinLength { get; set; }
    
    /// <summary>
    /// True if the input is required.
    /// </summary>
    public bool Required { get; set; }

    /// <summary>
    /// A regex pattern for validating the input.
    /// </summary>
    public string? Pattern { get; set; }
}
