
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class TextBoxPage : BasePage
{
    private readonly ILocator _textArea;
    private readonly ILocator _characterCountError;

    public TextBoxPage(IPage page) : base(page)
    {
        _textArea = page.Locator(".govuk-textarea");
        _characterCountError = page.Locator("div.govuk-character-count__status");
    }

    private async Task EnterTextAreaAndContinue()
    {
        var randomText = Guid.NewGuid().ToString("N").Substring(0, 20);
        await _textArea.ClearAsync();
        await _textArea.FillAsync(randomText);
        await SaveAndContinue();
    }

    public async Task EnterText(String text)
    {
        await _textArea.ClearAsync();
        await _textArea.FillAsync(text);
    }

    public async Task VerifyCharacterCountError(String expectedError)
    {
        await Expect(_characterCountError).ToContainTextAsync(expectedError);
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
