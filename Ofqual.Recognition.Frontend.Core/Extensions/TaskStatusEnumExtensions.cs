using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Extensions;

public static class TaskStatusEnumExtensions
{
    /// <summary>
    /// Maps the task status to a GOV.UK tag class.
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

    /// <summary>
    /// Maps the task status to a user-friendly label.
    /// </summary>
    public static string GetDisplayName(this TaskStatusEnum status)
    {
        return status switch
        {
            TaskStatusEnum.NotStarted => "Not started",
            TaskStatusEnum.InProgress => "In progress",
            TaskStatusEnum.Completed => "Completed",
            TaskStatusEnum.CannotStartYet => "Cannot start yet",
            _ => "Unknown"
        };
    }
}