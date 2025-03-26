using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages
{
    public class Question3Page : BasePage
    {
        private readonly ILocator _continueButton;


        public Question3Page(IPage page) : base(page)
        {
            _continueButton = page.Locator("button.govuk-button:has-text('Continue')");
        }

        public async Task ClickContinueButton()
        {
            await _continueButton.ClickAsync();
        }
    }
}