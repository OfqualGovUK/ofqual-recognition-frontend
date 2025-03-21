using System.ComponentModel.DataAnnotations;
using Ofqual.Recognition.Frontend.Core.ViewModels.Interfaces;

namespace Ofqual.Recognition.Frontend.Core.ViewModels;

public class QuestionThree : IEligibilityQuestions
{
    [Required(ErrorMessage = "You need to select an option to continue.")]
    public string Answer { get; set; } = string.Empty;
}