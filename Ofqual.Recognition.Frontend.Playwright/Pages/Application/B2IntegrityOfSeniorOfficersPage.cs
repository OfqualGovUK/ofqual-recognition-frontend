
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class B2IntegrityOfSeniorOfficersPage : BasePage
{
    public B2IntegrityOfSeniorOfficersPage(IPage page) : base(page)
    {

    }

    public async Task CompleteB2IntegrityOfSeniorOfficersTask(TaskListPage taskListPage)
    {
        await taskListPage.CheckTaskStatus("Criteria B.2 - Declaration of integrity of senior officers", "Not started");
        await taskListPage.ClickTaskLink("Criteria B.2 - Declaration of integrity of senior officers");
        await RunAxeCheck();
        await SelectRadioButtonByValue("I have not identified any concerns relating to the integrity of the proposed Senior Officers.");
        await SaveAndContinue();
        await RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Criteria B.2 - Declaration of integrity of senior officers", "Completed");
    }
}
