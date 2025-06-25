namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionAnswerListViewModel : TaskListViewModel
{
    public bool IsReadOnly { get; set; } = false;

    // List of questions and their metadata
    public List<QuestionAnswerItemViewModel> Items { get; set; } = new();
}

public class QuestionAnswerItemViewModel
{
    public string Question { get; set; }
    public string Answer { get; set; }
    public string ChangeUrl { get; set; }
}