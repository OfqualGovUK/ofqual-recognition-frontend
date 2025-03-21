using System.ComponentModel.DataAnnotations;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;
public class QuestionThreeViewModel
{
    [Required(ErrorMessage = "You need to select an option to continue.")]
    public string Answer { get; set; } = string.Empty;
}