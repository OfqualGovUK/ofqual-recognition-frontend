namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class BodyItemViewModel
{
    /// <summary>
    /// The type of content, such as "heading", "paragraph", "list", or "button".
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Main text content. Used in headings, paragraphs and labels.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// List of bullet points. Used when the type is list.
    /// </summary>
    public List<string>? Items { get; set; }

    /// <summary>
    /// Button details, used when the type is button.
    /// </summary>
    public ButtonViewModel? Button { get; set; }

    /// <summary>
    /// Text shown before a link in a paragraph.
    /// </summary>
    public string? TextBeforeLink { get; set; }

    /// <summary>
    /// Text shown after a link in a paragraph.
    /// </summary>
    public string? TextAfterLink { get; set; }

    /// <summary>
    /// A single link used inside a paragraph.
    /// </summary>
    public LinkViewModel? Link { get; set; }
}