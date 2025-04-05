using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IEligibilityService
{
    Eligibility GetAnswers();
    Question GetQuestion(string sessionKey);
}
