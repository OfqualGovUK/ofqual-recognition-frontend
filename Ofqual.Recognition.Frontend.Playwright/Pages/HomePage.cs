using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages
{
    public class HomePage : BasePage
    {
        private readonly string _baseUrl;
        private readonly ILocator _startButton;

        public HomePage(IPage page) : base(page)
        {
            _baseUrl = Environment.GetEnvironmentVariable("RecognitionBaseUrl") ?? "http://localhost:7159";
            _startButton = page.Locator(".govuk-button--start");
        }

        public async Task GoToHomePage()
        {
            await _page.GotoAsync(_baseUrl);
        }

        public async Task ClickStartButton()
        {
            await _startButton.ClickAsync();
        }
    }
}