
using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Service.Interfaces;

public interface IEligibilityService
{
    public Eligibility GetAnswers();
    public Question GetQuestion(string sessionKey);
}
