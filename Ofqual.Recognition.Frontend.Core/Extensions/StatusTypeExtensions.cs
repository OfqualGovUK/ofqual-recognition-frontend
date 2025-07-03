using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Extensions;

public static class StatusTypeExtensions
{
    /// <summary>
    /// Maps the task status to a GOV.UK tag class.
    /// </summary>
    public static string GetTagClass(this StatusType status)
    {
        return status switch
        {
            StatusType.Completed => "govuk-tag--green",
            StatusType.InProgress => "govuk-tag--light-blue",
            StatusType.NotStarted => "govuk-tag--blue",
            StatusType.CannotStartYet => "govuk-tag--grey",
            _ => "govuk-tag--grey"
        };
    }

    /// <summary>
    /// Maps the task status to a user-friendly label.
    /// </summary>
    public static string GetDisplayName(this StatusType status)
    {
        return status switch
        {
            StatusType.NotStarted => "Not started",
            StatusType.InProgress => "In progress",
            StatusType.Completed => "Completed",
            StatusType.CannotStartYet => "Cannot start yet",
            _ => "Unknown"
        };
    }
}