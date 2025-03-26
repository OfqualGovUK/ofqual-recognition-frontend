using System.ComponentModel.DataAnnotations;
using Ofqual.Recognition.Frontend.Core.Constants;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionOneViewModel
{
    [Required(ErrorMessage = "You need to select an option to continue.")]
    public string Answer { get; set; } = string.Empty;
    
    public string? ReturnUrl { get; set; }
    public string BackUrl => ReturnUrl == RouteConstants.EligibilityConstants.QUESTION_REVIEW_PATH
        ? RouteConstants.EligibilityConstants.QUESTION_REVIEW_PATH
        : RouteConstants.EligibilityConstants.START_PATH;
}