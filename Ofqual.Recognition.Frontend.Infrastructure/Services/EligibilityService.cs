using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class EligibilityService : IEligibilityService
{
    private readonly ISessionService _sessionService;

    public EligibilityService(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public Eligibility GetAnswers()
    {
        return new Eligibility
        {
            QuestionOne = _sessionService.GetFromSession<string>(SessionKeys.EligibilityQuestionOne) ?? string.Empty,
            QuestionTwo = _sessionService.GetFromSession<string>(SessionKeys.EligibilityQuestionTwo) ?? string.Empty,
            QuestionThree = _sessionService.GetFromSession<string>(SessionKeys.EligibilityQuestionThree) ?? string.Empty,
        };
    }

    public Question GetQuestion(string sessionKey)
    {
        var answer = _sessionService.GetFromSession<string>(sessionKey) ?? string.Empty;
        return new Question { Answer = answer }; 
    }
}