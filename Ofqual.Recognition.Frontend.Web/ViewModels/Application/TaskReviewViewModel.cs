using Ofqual.Recognition.Frontend.Core.Validations;
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskReviewViewModel
{
    public TaskReviewSectionViewModel[] SectionHeadings { get; set; } = Array.Empty<TaskReviewSectionViewModel>();

    [ValidEnumValue(typeof(TaskStatusEnum), ErrorMessage = "The status provided is not valid.")]

    public TaskStatusEnum Answer { get; set; }
}

public class TaskReviewSectionViewModel
{
    public string Title { get; set; } = string.Empty;
    public TaskReviewItemViewModel[] Items { set; get; } = Array.Empty<TaskReviewItemViewModel>();
}

public class TaskReviewItemViewModel
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
