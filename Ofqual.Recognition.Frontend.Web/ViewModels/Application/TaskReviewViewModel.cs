using Ofqual.Recognition.Frontend.Core.Validations;
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskReviewViewModel
{
    public TaskReviewSectionHeadingViewModel[] SectionHeadings { get; set; } = Array.Empty<TaskReviewSectionHeadingViewModel>();

    [ValidEnumValue(typeof(TaskStatusEnum), ErrorMessage = "The status provided is not valid.")]

    public TaskStatusEnum ApplicationTaskStatus { get; set; }
}

public class TaskReviewSectionHeadingViewModel
{
    public string Title { get; set; } = string.Empty;
    public TaskReviewSectionItemViewModel[] Items { set; get; } = Array.Empty<TaskReviewSectionItemViewModel>();
}

public class TaskReviewSectionItemViewModel
{
    public string Title { get; set; } = string.Empty;
   
    public TaskReviewQuestionAnswerViewModel[] QuestionAnswers { set; get; } = Array.Empty<TaskReviewQuestionAnswerViewModel>();
}

public class TaskReviewQuestionAnswerViewModel
{
    public string Question { get; set; } = string.Empty;     

    public string Answer { get; set; } = "Not Provided";

    public string QuestionURL { get; set; } = "#";
}
