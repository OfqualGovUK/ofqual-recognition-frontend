namespace Ofqual.Recognition.Frontend.Web.ViewModels;
public class QuestionContentViewModel
{
    public string? Title { get; set; }
    public List<BodyItemViewModel>? Body { get; set; }
    public List<HelpItemViewModel>? Help { get; set; }
    public FormGroupViewModel? FormGroup { get; set; }
}