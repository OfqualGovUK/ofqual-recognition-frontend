
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class A2A3ConstitutionPage : BasePage
{
    public A2A3ConstitutionPage(IPage page) : base(page)
    {
    }

    public async Task CompleteA2A3Task(TaskListPage taskListPage, HomePage homePage)
    {
        await taskListPage.CheckTaskStatus("Criteria A.2 and A.3 - Constitution", "Not started");
        await taskListPage.ClickTaskLink("Criteria A.2 and A.3 - Constitution");
        await homePage.RunAxeCheck();
        await SelectRadioButtonByValue("Yes");
        await SaveAndContinue();
        await homePage.RunAxeCheck();
        await SelectRadioButtonByValue("Yes");
        await SaveAndContinue();
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Criteria A.2 and A.3 - Constitution", "Completed");
    }
}
