using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers
{
    public class TaskReviewSection
    {
        public string SectionName { get; set; }
        public List<TaskReviewGroup> TaskGroups { get; set; } = new List<TaskReviewGroup>();
    }
}
