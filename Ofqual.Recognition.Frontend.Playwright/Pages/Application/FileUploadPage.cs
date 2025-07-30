
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class FileUploadPage : BasePage
{
    private readonly ILocator _ChooseFilesButton;
    private readonly ILocator _UploadFilesButton;
    private readonly ILocator _SubmitFilesButton;
    public FileUploadPage(IPage page) : base(page)
    {
        _ChooseFilesButton = page.Locator("#files-input");
        _UploadFilesButton = page.Locator("#submit-form-group");
        _SubmitFilesButton = page.GetByText("Submit files");
    }

    public async Task ChooseFilesAndUpload(String fileName)
    {
        var testdataFolder = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
        var filePaths = new[]
        {
            Path.Combine(testdataFolder, fileName)
        };
        await _ChooseFilesButton.SetInputFilesAsync(filePaths);
        await _UploadFilesButton.ClickAsync();
        await _SubmitFilesButton.ClickAsync();
    }

    public async Task CompleteFileUploadTask(TaskListPage taskListPage, HomePage homePage, String taskName, String fileName)
    {
        await taskListPage.CheckTaskStatus(taskName, "Not started");
        await taskListPage.ClickTaskLink(taskName);
        await homePage.RunAxeCheck();
        await ChooseFilesAndUpload(fileName);
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus(taskName, "Completed");
    }
}
