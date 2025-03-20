using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Extensions;

public static class TaskStatusEnumExtensions
{
    /// <summary>
    /// Gets the corresponding GOV.UK tag CSS class for a given task status.
    /// </summary>
    public static string GetTagClass(this TaskStatusEnum status)
    {
        return status switch
        {
            TaskStatusEnum.Completed => "govuk-tag--green",
            TaskStatusEnum.InProgress => "govuk-tag--light-blue",
            TaskStatusEnum.NotStarted => "govuk-tag--blue",
            TaskStatusEnum.CannotStartYet => "govuk-tag--grey",
            _ => "govuk-tag--grey"
        };
    }
}