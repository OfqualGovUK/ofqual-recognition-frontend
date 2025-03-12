using Microsoft.AspNetCore.Http;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services
{
    public class EligibilityService : IEligibilityService
    {
        private readonly ISession _session;

        public EligibilityService(IHttpContextAccessor httpContextAccessor) => _session = httpContextAccessor!.HttpContext!.Session;

        public void SaveAnswers(string? questionOne, string? questionTwo, string? questionThree)
        {
            if (!string.IsNullOrEmpty(questionOne))
            {
                _session.SetString("QuestionOne", questionOne);
            }

            if (!string.IsNullOrEmpty(questionTwo))
            {
                _session.SetString("QuestionTwo", questionTwo);
            }

            if (!string.IsNullOrEmpty(questionThree))
            {
                _session.SetString("QuestionThree", questionThree);
            }
        }

        public EligibilityModel GetAnswers()
        {
            return new EligibilityModel
            {
                QuestionOne = _session.GetString("QuestionOne") ?? string.Empty,
                QuestionTwo = _session.GetString("QuestionTwo") ?? string.Empty,
                QuestionThree = _session.GetString("QuestionThree") ?? string.Empty
            };
        }
    }
}
