using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers
{
    public class TaskReviewItem
    {
        public string? QuestionText { get; set; }
        public List<string>? AnswerValue { get; set; }
        public string? QuestionUrl { get; set; }
    }
}
