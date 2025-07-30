
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class TextBoxPage : BasePage
{
    private readonly ILocator _textArea;
    public TextBoxPage(IPage page) : base(page)
    {
        _textArea = page.Locator(".govuk-textarea");
    }

    public async Task EnterTextAreaAndContinue()
    {
        var randomText = Guid.NewGuid().ToString("N").Substring(0, 20);
        await _textArea.ClearAsync();
        await _textArea.FillAsync(randomText);
        await SaveAndContinue();
    }

    public async Task CompleteTextAreaTask(TaskListPage taskListPage, HomePage homePage, String taskName)
    {
        await taskListPage.CheckTaskStatus(taskName, "Not started");
        await taskListPage.ClickTaskLink(taskName);
        await homePage.RunAxeCheck();
        await EnterTextAreaAndContinue();
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus(taskName, "Completed");
    }
}
