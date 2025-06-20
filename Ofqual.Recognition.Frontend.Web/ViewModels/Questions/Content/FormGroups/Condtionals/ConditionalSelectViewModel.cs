namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class ConditionalSelectViewModel
{
    /// <summary>
    /// The select dropdown to render when the parent option is selected.
    /// </summary>
    public required SelectViewModel Select { get; set; }

    /// <summary>
    /// The user's existing answers, used to populate the selected value.
    /// </summary>
    public string? AnswerJson { get; set; }

    /// <summary>
    /// A list of validation errors related to the dropdown.
    /// </summary>
    public IEnumerable<ErrorItemViewModel>? Errors { get; set; }
}
