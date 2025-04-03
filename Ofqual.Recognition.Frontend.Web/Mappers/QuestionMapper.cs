using Newtonsoft.Json;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.ViewModels;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public static class QuestionMapper
{
    public static QuestionViewModel MapToViewModel(QuestionResponse question)
    {
        var json = JsonConvert.DeserializeObject<QuestionContent>(question.QuestionContent);
        var questionViewModel = new QuestionViewModel
        {
            QuestionTypeName = question.QuestionTypeName,
            QuestionId = question.QuestionId,
            QuestionContent = new QuestionContent
            {
                Title = json.Title,
                Hint = json?.Hint,
                Label = json?.Label
            }
        };
        return questionViewModel;
    }
}