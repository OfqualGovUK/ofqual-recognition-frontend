using Ofqual.Recognition.Frontend.Playwright.Pages;
using Playwright.Axe;

namespace Ofqual.Recognition.Frontend.Playwright.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class EligibilityQuestions : PageTest
{
    [Test]
    public async Task EligibleDecision()
    {
        var homePage = new HomePage(Page);
        var checkOrgaisationEligibilityPage = new CheckOrgaisationEligibilityPage(Page);
        var question1Page = new Question1Page(Page);
        var question2Page = new Question2Page(Page);
        var question3Page = new Question3Page(Page);
        var checkYourAnswersPage = new CheckYourAnswersPage(Page);
        var eligibilityDecisionPage = new EligibilityDecisionPage(Page);

        //Homepage
        await homePage.GoToHomePage();
        await homePage.CheckPageHeading("Apply to have your qualifications recognised");
        await homePage.ClickStartButton();

        //Check organisation eligibility page
        await checkOrgaisationEligibilityPage.CheckPageHeading("Check if your organisation is eligible to apply for Ofqual recognition");
        await checkOrgaisationEligibilityPage.ClickContinueButton();

        //Qualifications we regulate
        await question1Page.CheckPageHeading("Qualifications we regulate");
        await question1Page.CheckQuestionHeading("Do you award or intend to award qualifications?");
        await question1Page.SelectRadioButtonByValue("Yes");
        await question1Page.ClickContinueButton();

        //Is your organisation based in the UK, Gibraltar or a European Union page
        await question2Page.CheckPageHeading("Is your organisation based in the UK, Gibraltar or a European Union or European Free Trade Association member state, or if not, does your organisation have a substantial presence in one of those territories?");
        await question2Page.SelectRadioButtonByValue("Yes");
        await question2Page.ClickContinueButton();

        //Do you intend to make qualifications available to learners in England?
        await question3Page.CheckPageHeading("Do you intend to make qualifications available to learners in England?");
        await question3Page.SelectRadioButtonByValue("Yes");
        await question3Page.ClickContinueButton();

        //check your answers
        await checkYourAnswersPage.CheckPageHeading("Check your answers");

        var questionsAndAnswers = new List<(string Question, string ExpectedAnswer)>
        {
            ("Do you award or intend to award qualifications?", "Yes"),
            ("Is your organisation based in the UK, Gibraltar or a European Union or European Free Trade Association member state, or if not, does your organisation have a substantial presence in one of those territories?", "Yes"),
            ("Do you intend to make qualifications available to learners in England?", "Yes")
        };

        foreach (var (question, expectedAnswer) in questionsAndAnswers)
        {
            await checkYourAnswersPage.CheckSummaryListContent(question, expectedAnswer);
        }
        await checkYourAnswersPage.ClickContinueButton();

        // Eligibility decision page
        await eligibilityDecisionPage.CheckPageHeading("Eligible");
        await eligibilityDecisionPage.VerifyCreateAccountLink();

        // run an axe accessibility scan on the page
        AxeHtmlReportOptions reportOptions = new(reportDir: ".//reports");
        AxeResults axeResults = await Page.RunAxe(reportOptions: reportOptions);
        Console.WriteLine($"Axe ran against {axeResults.Url} on {axeResults.Timestamp}.");
    }

    [Test]
    public async Task NotEligibleDecision()
    {
        var homePage = new HomePage(Page);
        var checkOrgaisationEligibilityPage = new CheckOrgaisationEligibilityPage(Page);
        var question1Page = new Question1Page(Page);
        var question2Page = new Question2Page(Page);
        var question3Page = new Question3Page(Page);
        var checkYourAnswersPage = new CheckYourAnswersPage(Page);
        var eligibilityDecisionPage = new EligibilityDecisionPage(Page);

        //Homepage
        await homePage.GoToHomePage();
        await homePage.ClickStartButton();

        //Check organisation eligibility page
        await checkOrgaisationEligibilityPage.ClickContinueButton();

        //Qualifications we regulate
        await question1Page.SelectRadioButtonByValue("No");
        await question1Page.ClickContinueButton();

        //Is your organisation based in the UK, Gibraltar or a European Union page
        await question2Page.SelectRadioButtonByValue("Yes");
        await question2Page.ClickContinueButton();

        //Do you intend to make qualifications available to learners in England?
        await question3Page.SelectRadioButtonByValue("Yes");
        await question3Page.ClickContinueButton();

        //check your answers
        var questionsAndAnswers = new List<(string Question, string ExpectedAnswer)>
        {
            ("Do you award or intend to award qualifications?", "No"),
            ("Is your organisation based in the UK, Gibraltar or a European Union or European Free Trade Association member state, or if not, does your organisation have a substantial presence in one of those territories?", "Yes"),
            ("Do you intend to make qualifications available to learners in England?", "Yes")
        };

        foreach (var (question, expectedAnswer) in questionsAndAnswers)
        {
            await checkYourAnswersPage.CheckSummaryListContent(question, expectedAnswer);
        }
        await checkYourAnswersPage.ClickContinueButton();

        // Eligibility decision page
        await eligibilityDecisionPage.CheckPageHeading("Not Eligible");
        await eligibilityDecisionPage.VerifyNotEligiblePage();

        // run an axe accessibility scan on the page
        AxeHtmlReportOptions reportOptions = new(reportDir: ".//reports");
        AxeResults axeResults = await Page.RunAxe(reportOptions: reportOptions);
        Console.WriteLine($"Axe ran against {axeResults.Url} on {axeResults.Timestamp}.");
    }
}
