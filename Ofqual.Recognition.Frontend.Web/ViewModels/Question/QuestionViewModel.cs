using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Web.ViewModels.Question;

public class QuestionViewModel
{
    // Currently in discussions about this content
    public Guid QuestionId { get; set; }
    public Guid QuestionTypeId { get; set; }
    public QuestionContent QuestionContent { get; set; }
}

public class QuestionContent
{
    public string Title { get; set; }
    public string Hint { get; set; }
}


//{
//    "Title" : "What qualifications or end-point assessments you wish to be regulated for?",
//    "Hint" : "Please give details of the specific qualifications, end-point assessments or descriptions of qualifications you wish to offer. Referring to SSAs and qualification levels where possible.",
//    "Rules" : {
//        "MaxLength" : "500",
//        "MinLength" : "100",
//        "Required" : "true"
//    }
//}