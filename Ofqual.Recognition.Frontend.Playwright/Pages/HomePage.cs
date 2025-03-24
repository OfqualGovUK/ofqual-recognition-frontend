using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages;

public class HomePage : PageTest
{
    private readonly IPage _page;
    private readonly string _baseUrl;
    private readonly ILocator _heading;

    public HomePage(IPage Page)
    {
        _page = Page;
        _baseUrl = Environment.GetEnvironmentVariable("RecognitionBaseUrl") ?? "http://localhost:7159";
        _heading = Page.Locator("h1.govuk-heading-xl");
    }

    public async Task GoToHomePage()
    {
        await _page.GotoAsync(_baseUrl);
    }

    public async Task checkPageHeading(string heading)
    {
        await Expect(_heading).ToHaveTextAsync(heading);
    }
}
