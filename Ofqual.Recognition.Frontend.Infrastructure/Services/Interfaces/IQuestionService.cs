﻿using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces
{
    public interface IQuestionService
    {
        public Task<QuestionResponse>GetQuestionDetails(string taskName, string questionName);
    }
}
