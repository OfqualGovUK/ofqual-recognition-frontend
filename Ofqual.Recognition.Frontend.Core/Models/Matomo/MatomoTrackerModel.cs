namespace Ofqual.Recognition.Frontend.Core.Models;

public class MatomoTrackerModel
{
    public bool DisableCookieTimeoutExtension { get; set; } = true;
    public bool NoScriptTracking { get; set; } = true;
    public bool PrependDomainToTitle { get; set; } = true;
    public bool ClientDoNotTrackDetection { get; set; } = true;
}