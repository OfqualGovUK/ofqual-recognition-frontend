using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class TaskListPage : BasePage
{
    private readonly ILocator _taskList;
    public TaskListPage(IPage page) : base(page)
    {
        _taskList = _page.Locator("ul.govuk-task-list:has-text('Pre-application engagement')");
    }

    public async Task ClickTaskLink(string taskText)
    {
        await _page.Locator($"a.govuk-task-list__link:has-text('{taskText}')").ClickAsync();
    }

    public async Task CheckTaskStatus(string taskName, string expectedStatus)
    {
        var statusLocator = _page.Locator(
                    $"li.govuk-task-list__item:has(a.govuk-task-list__link:has-text('{taskName}')) .govuk-task-list__status"
                    );
        await Expect(statusLocator).ToContainTextAsync(expectedStatus.Trim());
    }

    public async Task WaitForPageToLoadAsync()
    {
        await _taskList.WaitForAsync();
    }
}
