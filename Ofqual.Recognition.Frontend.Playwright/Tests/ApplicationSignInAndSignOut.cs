using NUnit.Framework.Interfaces;
using Ofqual.Recognition.Frontend.Playwright.Pages;
using System.Globalization;
namespace Ofqual.Recognition.Frontend.Playwright.Tests;
[TestFixture]
public class ApplicationSignInAndSignOut : PageTest
{
    [TearDown]
    public async Task TakeScreenshotOnFailure()
    {
        var context = TestContext.CurrentContext;

        if (context.Result.Outcome.Equals(ResultState.Error))
            await Page.ScreenshotAsync(new()
            {
                FullPage = true,
                Path = $"Failure__{context.Test.FullName}_{DateTime.Now.ToString("o", CultureInfo.InvariantCulture)}.png"
            });
    }

    [Test]
    public async Task SignInAndSignOut()
    {
        var homePage = new HomePage(Page);

        //sign in

        await homePage.GoToHomePage();

        await homePage.CheckSignInAndSignOut();

      
    }
 
}
