using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.ViewModels;

namespace Ofqual.Recognition.Frontend.Web.Mappers;
public static class ApplicationMapper
{
    public static TaskReviewViewModel MapToViewModel() => 
        new TaskReviewViewModel()
        {
            Answer = TaskStatusEnum.InProgress,
            SectionHeadings =
            [
                new TaskReviewSectionViewModel
                {
                    Title = "Application Details",
                    Items =
                    [
                        new TaskReviewItemViewModel
                        {   Title = "Contact details",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel(){ Question = "Full name", QuestionURL = "application-details/contact-details" },
                                new TaskReviewQuestionAnswerViewModel(){ Question = "Email address", QuestionURL = "application-details/contact-details" },
                                new TaskReviewQuestionAnswerViewModel(){ Question = "Phone number", QuestionURL = "application-details/contact-details" },
                                new TaskReviewQuestionAnswerViewModel(){ Question = "Your role in the organisation", QuestionURL = "application-details/contact-details" }
                            ]
                        },
                        new TaskReviewItemViewModel { Title = "Organisation details",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel() { Question = "Organisation name", QuestionURL = "application-details/organisation-details" },
                                new TaskReviewQuestionAnswerViewModel() { Question = "Organisation legal name", QuestionURL = "application-details/organisation-details" },
                                new TaskReviewQuestionAnswerViewModel() { Question = "Acronym", QuestionURL = "application-details/organisation-details" },
                                new TaskReviewQuestionAnswerViewModel() { Question = "Website", QuestionURL = "application-details/organisation-details" }
                            ]
                         },
                        new TaskReviewItemViewModel { Title = "Organisation address", 
                            QuestionAnswers = 
                            [
                                 new TaskReviewQuestionAnswerViewModel() { Question = "Address line 1", QuestionURL = "application-details/organisation-address" },
                                 new TaskReviewQuestionAnswerViewModel() { Question = "Address line 2", QuestionURL = "application-details/organisation-address" },
                                 new TaskReviewQuestionAnswerViewModel() { Question = "Address line 3", QuestionURL = "application-details/organisation-address" },
                                 new TaskReviewQuestionAnswerViewModel() { Question = "Town or city", QuestionURL = "application-details/organisation-address" },
                                 new TaskReviewQuestionAnswerViewModel() { Question = "Postcode", QuestionURL = "application-details/organisation-address" },
                                 new TaskReviewQuestionAnswerViewModel() { Question = "Country", QuestionURL = "application-details/organisation-address" },
                            ]},
                        new TaskReviewItemViewModel { Title = "Qualifications or end-point assessments",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel 
                                { 
                                    Question = "What qualifications or end-point assessments you wish to be regulated for?",
                                    QuestionURL = "application-details/qualifications"
                                },
                            ] },
                        new TaskReviewItemViewModel { Title = "Ofqual recognition",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel 
                                { 
                                    Question = "Why do you want to be regulated by Ofqual?", QuestionURL="/application-details/why-regulated" 
                                },
                            ]
                        },
                    ]
                },
                new TaskReviewSectionViewModel
                {
                    Title = "Criteria A: Identity, constitution and governance",
                    Items =
                    [
                        new TaskReviewItemViewModel { Title = "Criteria A.1, A.2 and A.3 - Identity and Constitution",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel { Question = "Details of your organisation's legal entity" , QuestionURL = "criteria-a/type-of-organisation"},
                                new TaskReviewQuestionAnswerViewModel { Question = "Companies house number", QuestionURL = "criteria-a/type-of-organisation" },
                                new TaskReviewQuestionAnswerViewModel { Question = "Summarise how your organisation operates or plans to operate", QuestionURL = "criteria-a/type-of-organisation" },
                            ]
                        },
                        new TaskReviewItemViewModel { Title = "Criteria A.4 - Organisation and governance",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel { Question = "Summarise how your current or proposed organisational structure supports the development," +
                                    " delivery and awarding of qualifications and/or end-point assessment.",
                                    QuestionURL = "criteria-a/criteria-a4" },
                            ]
                        },
                        new TaskReviewItemViewModel { Title = "Criteria A.5 - Conflicts of interest",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel { Question = "Summarise how your organisation identifies, monitors, mitigates against, "+
                                    "and manages conflicts of interest",
                                    QuestionURL = "criteria-a/criteria-a5" },
                            ]
                        },
                        new TaskReviewItemViewModel { Title = "Criteria A.6 - Governing body oversight",
                            QuestionAnswers =
                            [
                                new TaskReviewQuestionAnswerViewModel { Question = "Summarise the arrangements you have in place to ensure your governing" +
                                    " body is accountable for your organisation's activites and compliance as an awarding organisation",
                                    QuestionURL = "criteria-a/criteria-a6"
                                },
                            ]
                        },
                        new TaskReviewItemViewModel { Title = "Criteria A - Uploaded files" }
                    ]
                },
                new TaskReviewSectionViewModel
                {
                    Title = "Criteria B: Integrity",
                    Items = [
                        new TaskReviewItemViewModel { Title = "Criteria B.1 - Integrity of the applicant" , QuestionAnswers = [new TaskReviewQuestionAnswerViewModel { Question = "Declaration - integrity of the applicant", QuestionURL = "criteria-b/criterion-b1a" }]},
                        new TaskReviewItemViewModel { Title = "Criteria B.2 - Integrity of senior officers", QuestionAnswers = [new TaskReviewQuestionAnswerViewModel { Question = "Declaration - integrity of senior officers", QuestionURL = "criteria-b/criterion-b1b" }]},
                        new TaskReviewItemViewModel { Title = "Criteria B information", QuestionAnswers = [new TaskReviewQuestionAnswerViewModel { Question = "Summarise the policies and processes in place to ensure your organisation meets the criteria for the integrity of the organisation and it's senior officers" }] },
                        new TaskReviewItemViewModel { Title = "Criteria B - Uploaded files" }
                    ]
                },
                new TaskReviewSectionViewModel
                {
                    Title = "Criteria C: Resources and Financing",
                    Items =
                    [
                        new TaskReviewItemViewModel { Title = "Criterion C.1(a) - Systems, processes and resources", QuestionAnswers = [new TaskReviewQuestionAnswerViewModel { Question = "Summarise the arrangements you have in place to ensure you have the necessary systems, process and resources.", QuestionURL = "criteria-c/criterion-c1a" }] },
                        new TaskReviewItemViewModel { Title = "Criterion C.1(b) - Financial resources and facilities", QuestionAnswers = [new TaskReviewQuestionAnswerViewModel { Question = "Summarise the financial arrangements, resources and facilties you have in place", QuestionURL = "criteria-c/criterion-c1b" }] },
                        new TaskReviewItemViewModel { Title = "Criteria C - Uploaded files" }
                    ]
                },
                new TaskReviewSectionViewModel
                {
                    Title = "Criteria D: Competence",
                    Items =
                    [
                        new TaskReviewItemViewModel { Title = "Criterion D.1(a) - Development, delivery and awarding of qualifications", QuestionAnswers = [new TaskReviewQuestionAnswerViewModel { Question = "Summarise how you undertake the design, delivery and award of qualifications.", QuestionURL = "criteria-d/criterion-d1a" }] },
                        new TaskReviewItemViewModel { Title = "Criterion D.1(b) - Validity, Reliability, Comparability, Manageability and Minimising Bias", QuestionAnswers = [new TaskReviewQuestionAnswerViewModel { Question = "Summarise how you ensure your qualifications are valid, reliable, comparable, manageable and minimise bias.", QuestionURL = "criteria-d/criterion-d1b" }] },
                        new TaskReviewItemViewModel { Title = "Criterion D.1(c) - Qualification compatibility with Equalities Law", QuestionAnswers = [new TaskReviewQuestionAnswerViewModel { Question = "Summarise how you ensure your qualifications comply with Equalities Law.", QuestionURL = "criteria-d/criterion-d1c" }] },
                        new TaskReviewItemViewModel { Title = "Criteria D - Uploaded files" }
                    ]
                }
            ]
        };
    }


