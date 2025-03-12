using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces
{
    public interface IEligibilityService
    {
        void SaveAnswers(string questionOne, string questionTwo, string questionThree);
        EligibilityModel GetAnswers();
    }
}
