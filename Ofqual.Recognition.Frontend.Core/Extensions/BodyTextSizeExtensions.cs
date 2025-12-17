using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Extensions;

public static class BodyTextSizeExtensions
{
    /// <summary>
    /// Maps a BodyTextSize enum to the corresponding GOV.UK heading class.
    /// </summary>
    public static string ToGovUkHeadingClass(this BodyTextSize size)
    {
        return size switch
        {
            BodyTextSize.S => "govuk-heading-s",
            BodyTextSize.M => "govuk-heading-m",
            BodyTextSize.L => "govuk-heading-l",
            BodyTextSize.XL => "govuk-heading-xl",
            _ => "govuk-heading-m"
        };
    }

    /// <summary>
    /// Maps a BodyTextSize enum to the corresponding GOV.UK paragraph class.
    /// </summary>
    public static string ToGovUkTextClass(this BodyTextSize size)
    {
        return size switch
        {
            BodyTextSize.S => "govuk-body-s",
            BodyTextSize.M => "govuk-body",
            BodyTextSize.L => "govuk-body-l",
            BodyTextSize.XL => "govuk-body-l",
            _ => "govuk-body"
        };
    }

    /// <summary>
    /// Maps a BodyTextSize enum to a GOV.UK label size class.
    /// </summary>
    public static string ToGovUkLabelClass(this BodyTextSize size)
    {
        return size switch
        {
            BodyTextSize.S => "govuk-label--s",
            BodyTextSize.M => "govuk-label--m",
            BodyTextSize.L => "govuk-label--l",
            BodyTextSize.XL => "govuk-label--l",
            _ => "govuk-label--m"
        };
    }

    /// <summary>
    /// Maps a BodyTextSize enum to the GOV.UK fieldset legend size class.
    /// </summary>
    public static string ToGovUkLegendClass(this BodyTextSize size)
    {
        return size switch
        {
            BodyTextSize.S => "govuk-fieldset__legend--s",
            BodyTextSize.M => "govuk-fieldset__legend--m",
            BodyTextSize.L => "govuk-fieldset__legend--l",
            BodyTextSize.XL => "govuk-fieldset__legend--l",
            _ => "govuk-fieldset__legend--m"
        };
    }

    /// <summary>
    /// Gets the appropriate HTML heading tag for the given text size.
    /// </summary>
    public static string ToHeadingTag(this BodyTextSize size, HTMLHeading headingType)
        => headingType switch
        {
            HTMLHeading.H1 => "h1",
            HTMLHeading.H2 => "h2",
            HTMLHeading.H3 => "h3",
            HTMLHeading.H4 => "h4",
            HTMLHeading.H5 => "h5",
            HTMLHeading.H6 => "h6",
            _ => size switch
            {
                BodyTextSize.XL => "h1",
                BodyTextSize.L => "h2",
                BodyTextSize.M => "h3",
                BodyTextSize.S => "h4",
                _ => "h3"
            }
        };             
    }

