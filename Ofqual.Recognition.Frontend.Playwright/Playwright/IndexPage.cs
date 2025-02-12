
using Playwright.Axe;

namespace Ofqual.Recognition.Frontend.Playwright.Playwright
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {
        [Test]
        public async Task HomepageHasPlaywrightInTitleAndGetStartedLinkLinkingtoTheIntroPage()
        {
            await Page.GotoAsync("https://localhost:7072");


             //Runs Axe accessibility checks.
            AxeHtmlReportOptions reportOptions = new(reportDir: "./reports");
            AxeResults axeResults = await Page.RunAxe(reportOptions: reportOptions);
            Console.WriteLine($"Axe ran against {axeResults.Url} on {axeResults.Timestamp}.");
        }
    }
}
