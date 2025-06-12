using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace Ofqual.Recognition.Frontend.Playwright.Tests
{
    public class BaseTest : PageTest
    {

        [OneTimeSetUp]
        public void SetupScreenshotsFolder()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            var screenshotsDir = Path.Combine(projectRoot, "Screenshots");

            if (Directory.Exists(screenshotsDir))
            {
                foreach (var file in Directory.GetFiles(screenshotsDir))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(screenshotsDir);
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                var testName = TestContext.CurrentContext.Test.Name;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"fail_{testName}_{timestamp}.png";

                var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
                var screenshotsDir = Path.Combine(projectRoot, "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                var screenshotPath = Path.Combine(screenshotsDir, fileName);
                await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });

                TestContext.AddTestAttachment(screenshotPath, "Failure Screenshot");
            }
        }
    }
}
