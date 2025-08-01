
using Microsoft.Playwright;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class CriteriaA1IdentityPage : BasePage
{
    private readonly ILocator _companiesHouseCheckbox;
    private readonly ILocator _companiesHouseNumber;
    private readonly ILocator _charitiesCheckbox;
    private readonly ILocator _charitiesNumber;
    private readonly ILocator _publicBodyCheckbox;
    private readonly ILocator _selectPublicBodyOption;
    private readonly ILocator _individualOrSoletraderCheckbox;
    private readonly ILocator _registerInAnotherCountryCheckbox;
    private readonly ILocator _registeredCountry;
    private readonly ILocator _otherCountryNumber;

    public CriteriaA1IdentityPage(IPage page) : base(page)
    {
        _companiesHouseCheckbox = page.Locator("#typeOfOrganisation");
        _companiesHouseNumber = page.Locator("#registeredCompanyNumber");
        _charitiesCheckbox = page.Locator("#typeOfOrganisation-1");
        _charitiesNumber = page.Locator("#registeredCharityNumber");
        _publicBodyCheckbox = page.Locator("#typeOfOrganisation-2");
        _selectPublicBodyOption = page.Locator("#otherOrganisation");
        _individualOrSoletraderCheckbox = page.Locator("#typeOfOrganisation-3");
        _registerInAnotherCountryCheckbox = page.Locator("#typeOfOrganisation-4");
        _registeredCountry = page.Locator("#registeredCountry");
        _otherCountryNumber = page.Locator("#otherCountryNumber");
    }

    private async Task CompleteAllOrganisationTypes()
    {
        await _companiesHouseCheckbox.CheckAsync();
        await _companiesHouseNumber.FillAsync("1234567");

        await _charitiesCheckbox.CheckAsync();
        await _charitiesNumber.FillAsync("CHAR987");

        await _publicBodyCheckbox.CheckAsync();
        await _selectPublicBodyOption.SelectOptionAsync(new[] { "Public body" });

        await _individualOrSoletraderCheckbox.CheckAsync();

        await _registerInAnotherCountryCheckbox.CheckAsync();
        await _registeredCountry.FillAsync("France");
        await _otherCountryNumber.FillAsync("FR123456789");

        await SaveAndContinue();
    }

    public async Task CheckAllBoxes()
    {
        await _companiesHouseCheckbox.CheckAsync();
        await _charitiesCheckbox.CheckAsync();
        await _publicBodyCheckbox.CheckAsync();
        await _individualOrSoletraderCheckbox.CheckAsync();
        await _registerInAnotherCountryCheckbox.CheckAsync();
    }

    public async Task EnterInvalidCompanyNumbers()
    {
        await _companiesHouseNumber.FillAsync("12345674785");
        await _charitiesNumber.FillAsync("1");
        await _selectPublicBodyOption.SelectOptionAsync(new[] { "Public body" });
        await _registeredCountry.FillAsync("France");
        await _otherCountryNumber.FillAsync("FR123456789");
    }

    public async Task CompleteA1IdentityTask(TaskListPage taskListPage, HomePage homePage)
    {
        await taskListPage.CheckTaskStatus("Criteria A.1 - Identity", "Not started");
        await taskListPage.ClickTaskLink("Criteria A.1 - Identity");
        await homePage.RunAxeCheck();
        await CompleteAllOrganisationTypes();
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Criteria A.1 - Identity", "Completed");
    }
}
