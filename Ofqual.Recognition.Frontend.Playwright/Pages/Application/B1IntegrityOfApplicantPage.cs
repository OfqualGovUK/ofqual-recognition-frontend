
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class B1IntegrityOfApplicantPage : BasePage
{
    public B1IntegrityOfApplicantPage(IPage page) : base(page)
    {

    }

    public async Task CompleteB1IntegrityOfApplicantTask(TaskListPage taskListPage)
    {
        await taskListPage.CheckTaskStatus("Criteria B.1 - Declaration of integrity of the applicant", "Not started");
        await taskListPage.ClickTaskLink("Criteria B.1 - Declaration of integrity of the applicant");
        await RunAxeCheck();
        await SelectRadioButtonByValue("I have not identified any concerns relating to the integrity of the Applicant.");
        await SaveAndContinue();
        await RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Criteria B.1 - Declaration of integrity of the applicant", "Completed");
    }
}
