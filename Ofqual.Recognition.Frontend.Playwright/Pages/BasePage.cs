using Ofqual.Recognition.Frontend.Playwright.Configs;
using Microsoft.Playwright;
using Playwright.Axe;
using System.Text;

namespace Ofqual.Recognition.Frontend.Playwright.Pages
{
    public abstract class BasePage : PageTest
    {
        protected readonly IPage _page;
        protected readonly ILocator _heading;
        protected readonly string _baseUrl;
        protected readonly ILocator _saveAndContinueButton;

        private readonly List<AxeResults> _axeResultsList = new();
        protected BasePage(IPage page)
        {
            _baseUrl = TestConfig.RecognitionBaseUrl;
            _page = page;
            _heading = page.Locator("h1");
            _saveAndContinueButton = page.GetByRole(AriaRole.Button, new() { Name = "Save and continue" });
        }

        public async Task CheckPageHeading(string heading)
        {
            await Expect(_heading).ToHaveTextAsync(heading);
        }

        public async Task SelectRadioButtonByValue(string labelText)
        {
            var label = _page.Locator($".govuk-radios__label:has-text(\"{labelText}\")");
            var forAttribute = await label.GetAttributeAsync("for");
            var radioButton = _page.Locator($"input[id='{forAttribute}']");
            await radioButton.CheckAsync();
        }

        public async Task SaveAndContinue()
        {
            await _saveAndContinueButton.ClickAsync();
        }

        public async Task CheckYourAnswersContinue()
        {
            await SelectRadioButtonByValue("""Yes, I've completed this section""");
            await SaveAndContinue();
        }

        public async Task RunAxeCheck()
        {
            AxeHtmlReportOptions reportOptions = new(reportDir: ".//tempReports");
            var axeResults = await _page.RunAxe(reportOptions: reportOptions);
            Console.WriteLine($"Axe ran against {axeResults.Url} on {axeResults.Timestamp}.");
            _axeResultsList.Add(axeResults);
        }

        public async Task GenerateConsolidatedHtmlReport(string path)
        {
            var html = new StringBuilder();
            html.AppendLine("<html><head><title>Axe Accessibility Report</title></head><body>");
            html.AppendLine("<h1>Axe Accessibility Report</h1>");

            foreach (var axe in _axeResultsList)
            {
                html.AppendLine($"<section><h2>{axe.Url}</h2><p>{axe.Timestamp}</p>");
                html.AppendLine($"<p><strong>Passes:</strong> {axe.Passes.Count}</p>");
                html.AppendLine($"<p><strong>Violations:</strong> {axe.Violations.Count}</p>");
                html.AppendLine("<ul>");
                foreach (var v in axe.Violations)
                {
                    html.AppendLine($"<li>{v.Description} — Impact: {v.Impact} <br>");
                    html.AppendLine($"<a href='{v.HelpUrl}'>{v.HelpUrl}</a><ul>");
                    foreach (var node in v.Nodes)
                        html.AppendLine($"<li>{node.Html}</li>");
                    html.AppendLine("</ul></li>");
                }
                html.AppendLine("</ul></section>");
            }

            html.AppendLine("</body></html>");
            await File.WriteAllTextAsync(path, html.ToString());
            Console.WriteLine($"Report saved at {path}.");
        }
    }
}