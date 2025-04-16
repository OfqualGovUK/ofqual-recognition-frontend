using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public static class EligibilityMapper
{
    public static QuestionOneViewModel MapToQuestionOneViewModel(EligibilityQuestion question)
    {
        return new QuestionOneViewModel
        {
            Answer = question.Answer
        };
    }

    public static QuestionTwoViewModel MapToQuestionTwoViewModel(EligibilityQuestion question)
    {
        return new QuestionTwoViewModel
        {
            Answer = question.Answer
        };
    }

    public static QuestionThreeViewModel MapToQuestionThreeViewModel(EligibilityQuestion question)
    {
        return new QuestionThreeViewModel
        {
            Answer = question.Answer
        };
    }
    
    public static EligibilityViewModel MapToEligibilityViewModel(Eligibility eligibility)
    {
        return new EligibilityViewModel
        {
            QuestionOne = eligibility.QuestionOne,
            QuestionTwo = eligibility.QuestionTwo,
            QuestionThree = eligibility.QuestionThree
        };
    }
}