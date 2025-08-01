
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class ReviewPage : BasePage
{
    public ReviewPage(IPage page) : base(page)
    {

    }

    public async Task CompleteReviewTask(TaskListPage taskListPage, HomePage homePage)
    {
        await taskListPage.CheckTaskStatus("Review application", "Not started");
        await taskListPage.ClickTaskLink("Review application");
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Review application", "Completed");
    }
}
