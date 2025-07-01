using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers
{
    public class TaskReviewGroup
    {
        public string? SectionHeading { get; set; }
        public List<TaskReviewItem> QuestionAnswers { get; set; } = new List<TaskReviewItem>();
    }
}
