using Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers;

namespace Ofqual.Recognition.Frontend.Web.ViewModels.ApplicationAnswers
{
    public class TaskReviewSectionViewModel
    {
        public string SectionName { get; set; }
        public List<TaskReviewGroupViewModel> TaskGroups { get; set; } = new List<TaskReviewGroupViewModel>();
    }
}
