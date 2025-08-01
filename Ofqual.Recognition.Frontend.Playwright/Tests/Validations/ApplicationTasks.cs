using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Ofqual.Recognition.Frontend.Playwright.Pages;
using Ofqual.Recognition.Frontend.Playwright.Pages.Application;

namespace Ofqual.Recognition.Frontend.Playwright.Tests.Validations;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ApplicationTasks : BaseTest
{
    [Test]
    public async Task PreEngagementTaskValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var preApplicationEngagementPage = new PreApplicationEngagementPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //Pre-application engagement task checkbox validation
        await taskListPage.ClickTaskLink("Pre-application engagement");
        await preApplicationEngagementPage.ClickSendMeInformationButton();
        await preApplicationEngagementPage.ReturnToApplication();
        await taskListPage.ClickTaskLink("Pre-application engagement");
        await preApplicationEngagementPage.SaveAndContinue();
        await preApplicationEngagementPage.VerifyErrorMessage("Select Have you completed your pre-application engagement?");
    }

    [Test]
    public async Task TermsAndConditionsValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var termsAndConditionsPage = new TermsAndConditionsPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //Terms & conditions task checkbox validation
        await taskListPage.ClickTaskLink("Terms and conditions");
        await termsAndConditionsPage.SaveAndContinue();
        await termsAndConditionsPage.VerifyErrorMessage("Select Confirm you have read and understand the terms and conditions above.");
    }

    [Test]
    public async Task ContactDetailsValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var contactDetailsPage = new ContactDetailsPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //Contact details validation
        await taskListPage.ClickTaskLink("Contact details");
        await contactDetailsPage.SaveAndContinue();

        var expectedErrorMessages = new List<string> { "Enter Full name", "Enter Phone number", "Enter Your role in the organisation"};
        await contactDetailsPage.VerifyMultipleErrorMessages(expectedErrorMessages);
    }

    [Test]
    public async Task OrganisationDetailsValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var organisationDetailsPage = new OrganisationDetailsPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //Organisation details validation
        await taskListPage.ClickTaskLink("Organisation details");
        await organisationDetailsPage.SaveAndContinue();

        var expectedErrorMessages = new List<string> { "Enter Organisation name", "Enter Organisation legal name", "Enter Acronym", "Enter Organisation email address", "Enter Website" };
        await organisationDetailsPage.VerifyMultipleErrorMessages(expectedErrorMessages);
    }

    [Test]
    public async Task OrganisationNameAndAcronymValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var organisationDetailsPage = new OrganisationDetailsPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //Organisation details validation
        await taskListPage.ClickTaskLink("Organisation details");
        await organisationDetailsPage.CompleteOrganisationDetails("test organisation1", "TOL");

        var expectedErrorMessages = new List<string> { "The Organisation name \"test organisation1\" already exists in our records", "The Acronym \"TOL\" already exists in our records" };
        await organisationDetailsPage.VerifyMultipleErrorMessages(expectedErrorMessages);
    }

    [Test]
    public async Task OrganisationAddressDetailsValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var organisationDetailsPage = new OrganisationDetailsPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //Organisation details address validation
        await taskListPage.ClickTaskLink("Organisation details");
        await organisationDetailsPage.CompleteOrganisationDetails();
        await organisationDetailsPage.SaveAndContinue();
        await organisationDetailsPage.SaveAndContinue();

        var expectedErrorMessages = new List<string> { "Enter Address line 1", "Enter Town or city", "Enter Postcode", "Enter Country" };
        await organisationDetailsPage.VerifyMultipleErrorMessages(expectedErrorMessages);
    }

    [Test]
    public async Task A1IdentityValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var a1IdentityPage = new CriteriaA1IdentityPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //A.1 Identity validation
        //Error message on selection
        await taskListPage.ClickTaskLink("Criteria A.1 - Identity");
        await a1IdentityPage.SaveAndContinue();
        await a1IdentityPage.VerifyErrorMessage("Select Details of your organisation's legal entity");

        //Error messages on empty company numbers
        await a1IdentityPage.CheckAllBoxes();
        await a1IdentityPage.SaveAndContinue();
        var expectedErrorMessages = new List<string> { "Enter Registered company number", "Enter Registered charity number", "Enter Type of organisation", "Enter Country the organisation is registered in", "Enter Registered company number in that country" };
        await a1IdentityPage.VerifyMultipleErrorMessages(expectedErrorMessages);

        //Error message on company and charity number validation
        await a1IdentityPage.EnterInvalidCompanyNumbers();
        await a1IdentityPage.SaveAndContinue();
        var expectedErrors = new List<string> { "Registered company number must be 8 characters or fewer", "Registered charity number must be 6 characters or more" };
        await a1IdentityPage.VerifyMultipleErrorMessages(expectedErrors);
    }

    [Test]
    public async Task A23ConstitutionValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var a2a3ConstitutionPage = new A2A3ConstitutionPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //A.2, A.3 Identity validation
        //Error message on A.2
        await taskListPage.ClickTaskLink("Criteria A.2 and A.3 - Constitution");
        await a2a3ConstitutionPage.SaveAndContinue();
        await a2a3ConstitutionPage.VerifyErrorMessage("Select Is your organisation based in the UK, Gibraltar or an European Union or European Free Trade Association member state?");

        //Error messages on A.3
        await a2a3ConstitutionPage.SelectRadioButtonByValue("Yes");
        await a2a3ConstitutionPage.SaveAndContinue();
        await a2a3ConstitutionPage.SaveAndContinue();
        await a2a3ConstitutionPage.VerifyErrorMessage("Select Is your organisation properly constituted in accordance with law, and holds all registrations, authorisations, or approvals required to be held by an organisation of its type?");
    }

    [Test]
    public async Task B1TaskValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var b1IntegrityOfApplicantPage = new B1IntegrityOfApplicantPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //B.1 - Integrity of applicant validation
        await taskListPage.ClickTaskLink("Criteria B.1 - Declaration of integrity of the applicant");
        await b1IntegrityOfApplicantPage.SaveAndContinue();
        await b1IntegrityOfApplicantPage.VerifyErrorMessage("Select an option to confirm the results of your enquiries into the integrity of the applicant");
    }

    [Test]
    public async Task B2TaskValidation()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var b2IntegrityOfSeniorOfficersPage = new B2IntegrityOfSeniorOfficersPage(Page);

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        //B.1 - Integrity of applicant validation
        await taskListPage.ClickTaskLink("Criteria B.2 - Declaration of integrity of senior officers");
        await b2IntegrityOfSeniorOfficersPage.SaveAndContinue();
        await b2IntegrityOfSeniorOfficersPage.VerifyErrorMessage("Select an option to confirm the results of your enquiries into integrity of the proposed senior officers");
    }

    [TestCase("Qualifications or EPAs you want to be regulated for", "You have 620 words too many", "What qualifications or end-point assessments you wish to be regulated for? must be 500 words or fewer")]
    [TestCase("Why do you want to be regulated by Ofqual?", "You have 620 words too many", "Why do you want to be regulated by Ofqual? must be 500 words or fewer")]
    [TestCase("Criteria A.4 - Organisation and governance", "You have 620 words too many", "Summarise how your current or proposed organisational structure supports the development, delivery and awarding of qualifications. must be 500 words or fewer")]
    [TestCase("Criteria A.5 - Conflicts of interest", "You have 620 words too many", "Your summary must be 500 words or fewer")]
    [TestCase("Criteria A.6 - Governing body oversight", "You have 620 words too many", "Summarise the arrangements you have in place to ensure your Governing Body is accountable for and has oversight of your organisation's activities and compliance as an awarding organisation. must be 500 words or fewer")]
    [TestCase("Criterion C.1(a) - Systems, processes and resources", "You have 120 words too many", "Summarise the arrangements you have in place, or will have in place, to ensure you have the necessary systems, process and resources must be 1000 words or fewer")]
    [TestCase("Criterion C.1(b) - Financial resources and facilities", "You have 620 words too many", "Summarise the arrangements you have in place to ensure you have, or will have, financial resources and facilities must be 500 words or fewer")]
    [TestCase("Criterion D.1(a) - Development, delivery and awarding of qualifications", "You have 120 words too many", "Outline how you will undertake the design, delivery and award of qualifications, indicating where relevant evidence can be found in your submission. must be 1000 words or fewer")]
    [TestCase("Criterion D.1(b) - Validity, Reliability, Comparability, Manageability and Minimising Bias", "You have 120 words too many", "Outline how you ensure your qualifications are valid, reliable, comparable, manageable and minimise bias, indicating where relevant evidence can be found in your submission. must be 1000 words or fewer")]
    [TestCase("Criterion D.1(c) - Qualification compatibility with Equalities Law", "You have 120 words too many", "Outline how you ensure your qualifications comply with Equalities Law, indicating where relevant evidence can be found in your submission. must be 1000 words or fewer")]

    public async Task TextBoxValidationTest(string taskName, string characterCountError, string expectedErrorMessage)
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var textBoxPage = new TextBoxPage(Page);

        string text = "Qualifications and end point assessment to be regulated. ";
        string longText = string.Concat(Enumerable.Repeat(text, 140));

        await homePage.GoToHomePage();
        await homePage.ClickApplicationStartButton();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();

        await taskListPage.ClickTaskLink(taskName);
        await textBoxPage.EnterText(longText);
        await textBoxPage.VerifyCharacterCountError(characterCountError);
        await textBoxPage.SaveAndContinue();
        await textBoxPage.VerifyErrorMessage(expectedErrorMessage);
        await textBoxPage.VerifyCharacterCountError(characterCountError);
    }

}