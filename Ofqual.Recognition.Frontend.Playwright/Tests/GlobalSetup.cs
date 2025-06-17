namespace Ofqual.Recognition.Frontend.Playwright.Tests
{
    [SetUpFixture]
    public class GlobalSetup
    {
        [OneTimeSetUp]
        public void GlobalScreenshotsCleanup()
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
    }
}
