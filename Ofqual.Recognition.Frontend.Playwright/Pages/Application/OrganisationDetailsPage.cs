
using Microsoft.Playwright;
using Ofqual.Recognition.Frontend.Playwright.Configs;

namespace Ofqual.Recognition.Frontend.Playwright.Pages.Application;

public class OrganisationDetailsPage : BasePage
{
    private readonly ILocator _orgName;
    private readonly ILocator _orgLegalName;
    private readonly ILocator _acronym;
    private readonly ILocator _email;
    private readonly ILocator _website;
    private readonly ILocator _addressLine1;
    private readonly ILocator _townOrCity;
    private readonly ILocator _postCode;
    private readonly ILocator _country;

    public OrganisationDetailsPage(IPage page) : base(page)
    {
        _orgName = page.Locator("[id='organisationName']");
        _orgLegalName = page.Locator("[id='legalName']");
        _acronym = page.Locator("[id='acronym']");
        _email = page.Locator("[id='email']");
        _website = page.Locator("[id='website']");
        _addressLine1 = page.Locator("[id='addressLine1']");
        _townOrCity = page.Locator("[id='townCity']");
        _postCode = page.Locator("[id='postcode']");
        _country = page.Locator("[id='country']");
    }

    public async Task CompleteOrganisationDetails()
    {
        var now = DateTime.UtcNow;
        string uniqueSuffix = now.ToString("yyyyMMdd_HHmmss");
        string orgName = $"Test Organisation {uniqueSuffix}";
        string legalName = $"Test Organisation {uniqueSuffix} Ltd";
        string acronym = $"TEST{now.ToString("MMddHHmmss")}";
        string email = TestConfig.B2CUsername;
        string website = "www.google.com";

        await _orgName.ClearAsync();
        await _orgName.FillAsync(orgName);
        await _orgLegalName.ClearAsync();
        await _orgLegalName.FillAsync(legalName);
        await _acronym.ClearAsync();
        await _acronym.FillAsync(acronym);
        await _email.ClearAsync();
        await _email.FillAsync(email);
        await _website.ClearAsync();
        await _website.FillAsync(website);
        await SaveAndContinue();
    }

    public async Task CompleteOrganisationDetails(string orgName, string acronym)
    {
        var now = DateTime.UtcNow;
        string uniqueSuffix = now.ToString("yyyyMMdd_HHmmss");
        string legalName = $"Test Organisation {uniqueSuffix} Ltd";
        string email = TestConfig.B2CUsername;
        string website = "www.google.com";

        await _orgName.ClearAsync();
        await _orgName.FillAsync(orgName);
        await _orgLegalName.ClearAsync();
        await _orgLegalName.FillAsync(legalName);
        await _acronym.ClearAsync();
        await _acronym.FillAsync(acronym);
        await _email.ClearAsync();
        await _email.FillAsync(email);
        await _website.ClearAsync();
        await _website.FillAsync(website);
        await SaveAndContinue();
    }

    private async Task CompleteOrgAddressDetails(string address1, string townOrCity, string postcode, string country)
    {
        await _addressLine1.ClearAsync();
        await _addressLine1.FillAsync(address1);
        await _townOrCity.ClearAsync();
        await _townOrCity.FillAsync(townOrCity);
        await _postCode.ClearAsync();
        await _postCode.FillAsync(postcode);
        await _country.ClearAsync();
        await _country.FillAsync(country);
        await SaveAndContinue();
    }

    public async Task CompleteOrganisationDetailsTask(TaskListPage taskListPage, HomePage homePage)
    {
        await taskListPage.CheckTaskStatus("Organisation details", "Not started");
        await taskListPage.ClickTaskLink("Organisation details");
        await homePage.RunAxeCheck();
        await CompleteOrganisationDetails();
        await homePage.RunAxeCheck();
        await CompleteOrgAddressDetails("1 test street", "test city", "AA1 1AA", "United Kingdom");
        await homePage.RunAxeCheck();
        await CheckYourAnswersContinue();
        await taskListPage.CheckTaskStatus("Organisation details", "Completed");
    }

    
}
