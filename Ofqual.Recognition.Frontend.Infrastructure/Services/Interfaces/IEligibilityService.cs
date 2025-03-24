using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IEligibilityService
{
    public Eligibility GetAnswers();
    public Question GetQuestion(string sessionKey);
}
