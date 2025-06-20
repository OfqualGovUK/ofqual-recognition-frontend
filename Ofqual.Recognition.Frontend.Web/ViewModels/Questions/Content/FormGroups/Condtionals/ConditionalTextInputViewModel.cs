namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class ConditionalTextInputViewModel
{
    /// <summary>
    /// The text input to render when the parent option is selected.
    /// </summary>
    public required TextInputItemViewModel Input { get; set; }

    /// <summary>
    /// The user's existing answers, used to populate the input value.
    /// </summary>
    public string? AnswerJson { get; set; }

    /// <summary>
    /// A list of validation errors related to the input.
    /// </summary>
    public IEnumerable<ErrorItemViewModel>? Errors { get; set; }
}
