namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class FormGroupViewModel
{
    public TextareaViewModel? Textarea { get; set; }

    public RadioButtonGroupViewModel? RadioButtonGroup { get; set; }

    public CheckBoxGroupViewModel? CheckboxGroup { get; set; }

    public TextInputGroupViewModel? TextInputGroup { get; set; }

    public FileUploadViewModel? FileUpload { get; set; }
}