
using Ofqual.Recognition.Frontend.Playwright.Pages;

namespace Ofqual.Recognition.Frontend.Playwright.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class launchHomePage : PageTest
{

    [Test]
    public async Task LaunchHomePage()
    {
        var homePage = new HomePage(Page);

        await homePage.GoToHomePage(); ;
        await homePage.checkPageHeading("Apply to have your qualifications recognised");
    }

}
