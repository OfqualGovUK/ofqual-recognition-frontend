using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Extensions;

public static class BodyListStyleExtensions
{
    /// <summary>
    /// Maps a BodyListStyle enum to the GOV.UK list class.
    /// </summary>
    public static string ToGovUkClass(this BodyListStyle style)
    {
        return style switch
        {
            BodyListStyle.Numbered => "govuk-list--number",
            _ => "govuk-list--bullet"
        };
    }
}