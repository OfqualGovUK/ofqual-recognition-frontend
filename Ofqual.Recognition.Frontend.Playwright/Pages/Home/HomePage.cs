using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages
{
    public class HomePage : BasePage
    {
        private readonly string _basePath;
        private readonly ILocator _eligibilityStartButton;
        private readonly ILocator _applicationStartButton;

        public HomePage(IPage page) : base(page)
        {
            _basePath = "/home";
            _eligibilityStartButton = page.Locator("[data-test='eligibility-start']");
            _applicationStartButton = page.Locator("[data-test='application-start']");
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
    }
}