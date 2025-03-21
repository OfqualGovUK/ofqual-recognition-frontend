
using Ofqual.Recognition.Frontend.Core.ViewModels;
using Ofqual.Recognition.Frontend.Core.ViewModels.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Service.Interfaces;

public interface IEligibilityService
{
    public Eligibility GetAnswers();
    public T GetQuestion<T>(string sessionKey) where T : IEligibilityQuestions, new();
}
