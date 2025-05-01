using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages;

public class HomePage : BasePage
{
    private readonly string _basePath;
    private readonly ILocator _eligibilityStartButton;
    private readonly ILocator _applicationStartButton;
    private readonly ILocator _signInButton;
    private readonly ILocator _signOutButton;
    private readonly ILocator _userName;
    private readonly ILocator _passWord;
    private readonly ILocator _submitCredentials;
    private readonly ILocator _signedInText;

    public HomePage(IPage page) : base(page)
    {
        _basePath = "/home";
        _eligibilityStartButton = page.Locator("[data-test='eligibility-start']");
        _applicationStartButton = page.Locator("[data-test='application-start']");
        _signInButton =  page.GetByText("Sign In").Nth(1);
        _signOutButton =  page.GetByText("Sign Out");
        _userName = page.Locator("[id='email']");
        _passWord = page.Locator("[id='password']");
        _submitCredentials = page.Locator("[id=next]");
        _signedInText = page.GetByText("Sign in as QA");
    }

    public async Task GoToHomePage() 
    { 
        await _page.GotoAsync($"{_baseUrl.TrimEnd()}{_basePath.TrimStart()}");
    }

    public async Task ClickEligibilityStartButton()
    {
        await _eligibilityStartButton.ClickAsync();
    }
    public async Task CheckApplicationStartButton()
    {
        await _applicationStartButton.ClickAsync();
    }
    public async Task CheckSignInAndSignOut()
    {
        await _signInButton.ClickAsync();
        await _userName.ClickAsync();
        await _userName.FillAsync("qa.team@ofqual.gov.uk");
        //make password secret in the pipeline reference
        await _passWord.FillAsync("TheSkyIsNotGrey123?!"); 
        await _submitCredentials.ClickAsync();
        await _signedInText.IsVisibleAsync();
        await _signOutButton.ClickAsync();
        await _signedInText.IsHiddenAsync();
    }
    

}