using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IPreEngagementService
{
    public Task<PreEngagementQuestion?> GetFirstPreEngagementQuestion();
    public Task<PreEngagementQuestionDetails?> GetPreEngagementQuestionDetails(string taskNameUrl, string questionNameUrl);
}
