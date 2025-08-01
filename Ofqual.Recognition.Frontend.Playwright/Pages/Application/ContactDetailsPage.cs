
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class ContactDetailsPage : BasePage
{
    private readonly ILocator _fullName;
    private readonly ILocator _phoneNumber;
    private readonly ILocator _roleInOrg;

    public ContactDetailsPage(IPage page) : base(page)
    {
        _fullName = page.Locator("[id='fullName']");
        _phoneNumber = page.Locator("[id='phoneNumber']");
        _roleInOrg = page.Locator("[id='jobRole']");
    }

    private async Task CompleteContactDetailsForm(string fullName, string phoneNumber, string jobRole)
    {
        await _fullName.ClearAsync();
        await _fullName.FillAsync(fullName);
        await _phoneNumber.ClearAsync();
        await _phoneNumber.FillAsync(phoneNumber);
        await _roleInOrg.ClearAsync();
        await _roleInOrg.FillAsync(jobRole);
        await SaveAndContinue();
    }

    public async Task CompleteContactDetailsTask(TaskListPage taskListPage, HomePage homePage, string fullName, string phoneNumber, string jobRole)
    {
        await taskListPage.CheckTaskStatus("Contact details", "Not started");
        await taskListPage.ClickTaskLink("Contact details");
        await homePage.RunAxeCheck();
        await CompleteContactDetailsForm(fullName, phoneNumber, jobRole);
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Contact details", "Completed");
    }
}
