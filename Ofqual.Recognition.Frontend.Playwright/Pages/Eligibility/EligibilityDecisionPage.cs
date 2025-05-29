using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages;

public class EligibilityDecisionPage : BasePage
{
    private readonly ILocator _createAccountLink;
    private readonly ILocator _reviewAndChangeLink;

    public EligibilityDecisionPage(IPage page) : base(page)
    {
        _createAccountLink = page.Locator("a.govuk-button:has-text('Create an account')");
        _reviewAndChangeLink = page.Locator("a.govuk-link[href='question-review']");
    }

    public async Task VerifyCreateAccountLink()
    {
        await Expect(_createAccountLink).ToBeVisibleAsync();
        await Expect(_createAccountLink).ToHaveAttributeAsync("href", "https://recognition.ofqual.gov.uk");
    }

    public async Task VerifyNotEligiblePage()
    {
        await Expect(_reviewAndChangeLink).ToBeVisibleAsync();
    }
}
