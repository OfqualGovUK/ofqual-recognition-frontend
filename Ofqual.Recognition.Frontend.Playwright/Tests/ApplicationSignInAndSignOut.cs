using Ofqual.Recognition.Frontend.Playwright.Pages;

namespace Ofqual.Recognition.Frontend.Playwright.Tests;

[TestFixture]
public class ApplicationSignInAndSignOut : BaseTest
{
    [Test]
    public async Task SignInAndSignOut()
    {
        var homePage = new HomePage(Page);

        await homePage.GoToHomePage();
        await homePage.CheckSignInAndSignOut();      
    } 
}
