using Ofqual.Recognition.Frontend.Playwright.Pages;
using Ofqual.Recognition.Frontend.Playwright.Pages.Application;
using Playwright.Axe;

namespace Ofqual.Recognition.Frontend.Playwright.Tests.E2E;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ReviewApplication : BaseTest
{
    private readonly List<AxeResults> _axeResultsList = new();

    [Test]
    public async Task ReviewApplicationTask()
    {
        var homePage = new HomePage(Page);
        var taskListPage = new TaskListPage(Page);
        var preApplicationEngagementPage = new PreApplicationEngagementPage(Page);
        var termsAndConditionsPage = new TermsAndConditionsPage(Page);
        var contactDetailsPage = new ContactDetailsPage(Page);
        var organisationDetailsPage = new OrganisationDetailsPage(Page);
        var a1IdentityPage = new CriteriaA1IdentityPage(Page);
        var a2a3ConstitutionPage = new A2A3ConstitutionPage(Page);
        var b1IntegrityOfApplicantPage = new B1IntegrityOfApplicantPage(Page);
        var b2IntegrityOfSeniorOfficersPage = new B2IntegrityOfSeniorOfficersPage(Page);
        var reviewPage = new ReviewPage(Page);
        var fileUploadPage = new FileUploadPage(Page);
        var textBoxPage = new TextBoxPage(Page);

        //Homepage
        await homePage.GoToHomePage();
        await homePage.RunAxeCheck();
        await homePage.CheckPageHeading("Apply to have your qualifications regulated");
        await homePage.ClickApplicationStartButton();
        await homePage.RunAxeCheck();
        await homePage.Signin();

        await taskListPage.WaitForPageToLoadAsync();
        await homePage.RunAxeCheck();

        //Pre-application engagement task
        await preApplicationEngagementPage.CompletePreApplicationEngagementTask(taskListPage, homePage);

        //Terms and conditions task
        await termsAndConditionsPage.CompleteTermsAndConditionsTask(taskListPage, homePage);

        //Contact details task
        await contactDetailsPage.CompleteContactDetailsTask(taskListPage, homePage, "Test user", "+448012365478", "officer");

        //Organisation details task
        await organisationDetailsPage.CompleteOrganisationDetailsTask(taskListPage, homePage);

        //Qualifications details task
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Qualifications or EPAs you want to be regulated for");

        //Why do you want to be regulated by Ofqual?
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Why do you want to be regulated by Ofqual?");

        //Criteria A1, A2 & A3
        await a1IdentityPage.CompleteA1IdentityTask(taskListPage, homePage);
        await a2a3ConstitutionPage.CompleteA2A3Task(taskListPage, homePage);

        //Criteria A4, A.5, A.6 tasks
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Criteria A.4 - Organisation and governance");
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Criteria A.5 - Conflicts of interest");
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Criteria A.6 - Governing body oversight");

        //Criteria A - upload supporting evidence
        await fileUploadPage.CompleteFileUploadTask(taskListPage, homePage, "Criteria A - Upload supporting evidence", "JpegTest.jpeg");

        //Criteria B.1, B.2
        await b1IntegrityOfApplicantPage.CompleteB1IntegrityOfApplicantTask(taskListPage);
        await b2IntegrityOfSeniorOfficersPage.CompleteB2IntegrityOfSeniorOfficersTask(taskListPage);

        //Criteria C.1(a), C.1(b)
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Criterion C.1(a) - Systems, processes and resources");
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Criterion C.1(b) - Financial resources and facilities");

        //Criteria C - upload supporting evidence
        await fileUploadPage.CompleteFileUploadTask(taskListPage, homePage, "Criteria C - Upload supporting evidence", "file-sample_1MB.docx");


        //Criteria D.1(a), D.1(b), D.1(c)
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Criterion D.1(a) - Development, delivery and awarding of qualifications");
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Criterion D.1(b) - Validity, Reliability, Comparability, Manageability and Minimising Bias");
        await textBoxPage.CompleteTextAreaTask(taskListPage, homePage, "Criterion D.1(c) - Qualification compatibility with Equalities Law");

        //Criteria D - upload supporting evidence
        await fileUploadPage.CompleteFileUploadTask(taskListPage, homePage, "Criteria D - Upload supporting evidence", "file-example_PDF_1MB.pdf");

        //Review application task
        await reviewPage.CompleteReviewTask(taskListPage, homePage);
        
        await homePage.Signout();
        await homePage.RunAxeCheck();
        await homePage.GenerateConsolidatedHtmlReport(".//consolidatedAxeReport.html");

    }
}
