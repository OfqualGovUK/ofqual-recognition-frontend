using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages
{
    public class Question3Page : BasePage
    {
        private readonly ILocator _continueButton;
        private readonly ILocator _questionHeading;

        public Question3Page(IPage page) : base(page)
        {
            _continueButton = page.Locator("button.govuk-button:has-text('Continue')");
            _questionHeading = page.Locator("legend.govuk-fieldset__legend govuk-fieldset__legend--m");
        }

        public async Task ClickContinueButton()
        {
            await _continueButton.ClickAsync();
        }

        public async Task CheckQuestionHeading(string heading)
        {
            await Expect(_questionHeading).ToHaveTextAsync(heading);
        }
    }
}