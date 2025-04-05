namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class HelpItemViewModel
{
    public string? Name { get; set; }
    public string? RedirectUrl { get; set; }
    public string? Heading { get; set; }
    public string? Body { get; set; }

    public bool IsLink => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(RedirectUrl);
    public bool IsText => !string.IsNullOrEmpty(Heading) && !string.IsNullOrEmpty(Body);
}