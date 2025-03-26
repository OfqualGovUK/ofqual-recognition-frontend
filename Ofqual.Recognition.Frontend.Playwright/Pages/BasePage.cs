using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages
{
    public abstract class BasePage : PageTest
    {
        protected readonly IPage _page;
        protected readonly ILocator _heading;

        protected BasePage(IPage page)
        {
            _page = page;
            _heading = page.Locator("h1");
        }

        public async Task CheckPageHeading(string heading)
        {
            await Expect(_heading).ToHaveTextAsync(heading);
        }

        public async Task SelectRadioButtonByValue(string value)
        {
            var radioButton = _page.Locator($"input.govuk-radios__input[value='{value}']");
            await radioButton.CheckAsync();
        }
    }
}