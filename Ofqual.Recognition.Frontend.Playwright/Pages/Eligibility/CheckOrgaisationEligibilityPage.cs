using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages;

public class CheckOrgaisationEligibilityPage : BasePage
{
    protected readonly ILocator _continueButton;

    public CheckOrgaisationEligibilityPage(IPage page) : base(page)
    {
        _continueButton = page.Locator("a.govuk-button:has-text('Continue')");
    }

    public async Task ClickContinueButton()
    {
        await _continueButton.ClickAsync();
    }
}
