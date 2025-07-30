
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class TermsAndConditionsPage : BasePage
{
    public TermsAndConditionsPage(IPage page) : base(page)
    {

    }

    public async Task CompleteTermsAndConditionsTask(TaskListPage taskListPage, HomePage homePage)
    {
        await taskListPage.CheckTaskStatus("Terms and conditions", "Not started");
        await taskListPage.ClickTaskLink("Terms and conditions");
        await homePage.RunAxeCheck();
        await SelectRadioButtonByValue("Yes, I have read and understood the terms and conditions");
        await SaveAndContinue();
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Terms and conditions", "Completed");
    }
}
