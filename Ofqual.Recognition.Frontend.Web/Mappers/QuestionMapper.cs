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
            QuestionContent = new QuestionContentViewModel
            {
                Title = json?.Title,
                ButtonGroup = json?.ButtonGroup,
                Body = json?.Body,
                TextBox = json?.TextBox,
                Help = json?.Help
            }
        };
        return questionViewModel;
    }
}