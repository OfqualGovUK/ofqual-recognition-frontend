using NUnit.Framework.Interfaces;
using Ofqual.Recognition.Frontend.Playwright.Pages;
using System.Globalization;
namespace Ofqual.Recognition.Frontend.Playwright.Tests;
[TestFixture]
public class ApplicationSignInAndSignOut : PageTest
{
    [Test]
    public async Task SignInAndSignOut()
    {
        var homePage = new HomePage(Page);

        //sign in

        await homePage.GoToHomePage();

        await homePage.CheckSignInAndSignOut();      
    } 
}
