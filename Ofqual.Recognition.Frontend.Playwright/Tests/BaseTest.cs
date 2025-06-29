﻿using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace Ofqual.Recognition.Frontend.Playwright.Tests
{
    public class BaseTest : PageTest
    {
        [TearDown]
        public async Task TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                var testName = TestContext.CurrentContext.Test.Name;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var safeTestName = string.Concat(testName.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"fail_{safeTestName}_{timestamp}.png";

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
