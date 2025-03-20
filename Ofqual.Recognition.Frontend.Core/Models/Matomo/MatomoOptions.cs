namespace Ofqual.Recognition.Frontend.Core.Models;

public class MatomoOptions
{
    public Uri TrackerURL { get; set; } = default!;
    public int SiteId { get; set; }
    public MatomoTrackerOptions TrackerOptions { get; set; } = new MatomoTrackerOptions();
}
