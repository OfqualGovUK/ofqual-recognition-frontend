namespace Ofqual.Recognition.Frontend.Core.Models;

public class MatomoModel
{
    public Uri TrackerURL { get; set; } = default!;
    public int SiteId { get; set; }
    public MatomoTrackerModel TrackerOptions { get; set; } = new MatomoTrackerModel();
}
