
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class ContactDetailsPage : BasePage
{
    private readonly ILocator _firstName;
    private readonly ILocator _surName;
    private readonly ILocator _phoneNumber;
    private readonly ILocator _roleInOrg;

    public ContactDetailsPage(IPage page) : base(page)
    {
        _firstName = page.Locator("[id='firstName']");
        _surName = page.Locator("[id='surName']");
        _phoneNumber = page.Locator("[id='phoneNumber']");
        _roleInOrg = page.Locator("[id='jobRole']");
    }

    private async Task CompleteContactDetailsForm(string firstName, string surName, string phoneNumber, string jobRole)
    {
        await _firstName.ClearAsync();
        await _firstName.FillAsync(firstName);
        await _surName.ClearAsync();
        await _surName.FillAsync(surName);
        await _phoneNumber.ClearAsync();
        await _phoneNumber.FillAsync(phoneNumber);
        await _roleInOrg.ClearAsync();
        await _roleInOrg.FillAsync(jobRole);
        await SaveAndContinue();
    }

    public async Task CompleteContactDetailsTask(TaskListPage taskListPage, HomePage homePage, string firstName, string surName, string phoneNumber, string jobRole)
    {
        await taskListPage.CheckTaskStatus("Contact details", "Not started");
        await taskListPage.ClickTaskLink("Contact details");
        await homePage.RunAxeCheck();
        await CompleteContactDetailsForm(firstName, surName, phoneNumber, jobRole);
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Contact details", "Completed");
    }
}
