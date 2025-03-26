using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages
{
    public class CheckYourAnswersPage : BasePage
    {
        private readonly ILocator _continueButton;
        private readonly ILocator _summaryList;

        public CheckYourAnswersPage(IPage page) : base(page)
        {
            _continueButton = page.Locator("button.govuk-button:has-text('Continue')");
            _summaryList = page.Locator("dl.govuk-summary-list");

        }

        public async Task ClickContinueButton()
        {
            await _continueButton.ClickAsync();
        }

        public async Task CheckSummaryListContent(string question, string expectedAnswer)
        {
            var row = _summaryList.Locator($"div.govuk-summary-list__row:has-text('{question}')");
            var answer = row.Locator("dd.govuk-summary-list__value");
            await Expect(answer).ToHaveTextAsync(expectedAnswer);

            var changeLink = row.Locator("dd.govuk-summary-list__actions a.govuk-link");
            await Expect(changeLink).ToHaveTextAsync(new Regex("^Change"));
        }
    }
}