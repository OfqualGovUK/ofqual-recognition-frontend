namespace Ofqual.Recognition.Frontend.Web.ViewModels.PreEngagement
{
    public class PreEngagementAnswerModel
    {
        public Guid QuestionId { get; set; }
        public Guid TaskId { get; set; }
        public string? AnswerJson { get; set; }
    }
}
