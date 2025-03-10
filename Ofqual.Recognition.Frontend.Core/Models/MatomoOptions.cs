namespace Ofqual.Recognition.Frontend.Web.Models
{
    public class MatomoOptions
    {
        public Uri TrackerURL { get; set; } = default!;
        public int SiteId { get; set; }
        public MatomoTrackerOptions TrackerOptions { get; set; } = new MatomoTrackerOptions();
    }

    public class MatomoTrackerOptions
    {
        public bool DisableCookieTimeoutExtension { get; set; } = true;
        public bool NoScriptTracking { get; set; } = true;
        public bool PrependDomainToTitle { get; set; } = true;
        public bool ClientDoNotTrackDetection { get; set; } = true;
    }
}
