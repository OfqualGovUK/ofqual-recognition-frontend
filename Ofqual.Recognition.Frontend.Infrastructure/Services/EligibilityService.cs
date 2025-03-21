using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.ViewModels;
using Ofqual.Recognition.Frontend.Core.ViewModels.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Service.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Service;

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
            QuestionOne = _sessionService.GetFromSession<string>(SessionKeys.QuestionOne) ?? string.Empty,
            QuestionTwo = _sessionService.GetFromSession<string>(SessionKeys.QuestionTwo) ?? string.Empty,
            QuestionThree = _sessionService.GetFromSession<string>(SessionKeys.QuestionThree) ?? string.Empty,
        };
    }

    public T GetQuestion<T>(string sessionKey) where T : IEligibilityQuestions, new()
    {
        var answer = _sessionService.GetFromSession<string>(sessionKey) ?? string.Empty;
        return new T { Answer = answer }; 
    }
}