using Ofqual.Recognition.Frontend.Playwright.Pages;
using Playwright.Axe;

namespace Ofqual.Recognition.Frontend.Playwright.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class launchHomePage : PageTest
{
    [Test]
    public async Task LaunchHomePage()
    {
        var homePage = new HomePage(Page);

        await homePage.GoToHomePage();
        await homePage.checkPageHeading("Apply to have your qualifications recognised");

        // run an axe accessibility scan on the page
        AxeHtmlReportOptions reportOptions = new(reportDir: ".//reports");
        AxeResults axeResults = await Page.RunAxe(reportOptions: reportOptions);
        Console.WriteLine($"Axe ran against {axeResults.Url} on {axeResults.Timestamp}.");
    }
}
