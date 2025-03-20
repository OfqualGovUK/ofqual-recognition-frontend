using System.ComponentModel.DataAnnotations;

namespace Ofqual.Recognition.Frontend.Core.Enums
{
    public enum TaskStatusEnum
    {
        [Display(Name = "Completed")]
        Completed = 1,
        
        [Display(Name = "In Progress")]
        InProgress = 2,

        [Display(Name = "Not Started")]
        NotStarted = 3,

        [Display(Name = "Cannot Start Yet")]
        CannotStartYet = 4
    }
}