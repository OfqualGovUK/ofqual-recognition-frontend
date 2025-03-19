using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofqual.Recognition.Frontend.Core.Models
{
    public class TaskItemStatusSectionDTOModel
    {
        public Guid SectionId { get; set; }
        public required string SectionName { get; set; }
        public int SectionOrderNumber { get; set; }
    }
}