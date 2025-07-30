
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class PreApplicationEngagementPage : BasePage
{
    private readonly ILocator _sendMeInformationButton;
    private readonly ILocator _returnToApplication;

    public PreApplicationEngagementPage(IPage page): base(page)
    {
        _sendMeInformationButton = page.Locator("[name='wantsEngagement']");
        _returnToApplication = page.GetByText("Return to application");
    }

    public async Task ClickSendMeInformationButton()
    {
        await _sendMeInformationButton.ClickAsync();
    }

    public async Task ReturnToApplication()
    {
        await _returnToApplication.ClickAsync();
    }

    public async Task CompletePreApplicationEngagementTask(TaskListPage taskListPage, HomePage homePage)
    {
        await taskListPage.CheckTaskStatus("Pre-application engagement", "Not started");
        await taskListPage.ClickTaskLink("Pre-application engagement");
        await homePage.RunAxeCheck();
        await ClickSendMeInformationButton();
        await homePage.RunAxeCheck();
        await ReturnToApplication();
        await taskListPage.CheckTaskStatus("Pre-application engagement", "In progress");
        await taskListPage.ClickTaskLink("Pre-application engagement");
        await SelectRadioButtonByValue("Yes");
        await SaveAndContinue();
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Pre-application engagement", "Completed");
    }
}
