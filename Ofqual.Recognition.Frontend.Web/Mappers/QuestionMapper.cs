using Newtonsoft.Json;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.ViewModels;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public static class QuestionMapper
{
    public static QuestionViewModel MapToViewModel(QuestionResponse question)
    {
        var json = JsonConvert.DeserializeObject<QuestionContentViewModel>(question.QuestionContent);
        var questionViewModel = new QuestionViewModel
        {
            QuestionTypeName = question.QuestionTypeName,
            QuestionId = question.QuestionId,
            TaskId = question.TaskId,
            QuestionContent = new QuestionContentViewModel
            {
                Heading = json?.Heading,
                Body = json?.Body,
                Help = json?.Help,
                FormGroup = json?.FormGroup
            }
        };
        return questionViewModel;
    }
}