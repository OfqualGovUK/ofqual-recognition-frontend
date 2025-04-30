using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Ofqual.Recognition.Frontend.Playwright.Configs;
using System.Diagnostics;
using System.Net.Http;

namespace Ofqual.Recognition.Frontend.Playwright.Pages
{
    public class HomePage : BasePage
    {
        private readonly string _basePath;
        private readonly ILocator _eligibilityStartButton;
        private readonly ILocator _applicationStartButton;

        public HomePage(IPage page) : base(page)
        {
            _basePath = Path.Join("/home");
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