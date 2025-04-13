namespace Ofqual.Recognition.Frontend.Core.Constants;

/// <summary>
/// Conventions for URL and Page references in this codebase:
/// </summary>
/// <remarks>
/// - <b>PATHs:</b> These relate to URLs and should be in lower case.
/// - <b>PAGEs:</b> These relate to Pages and should be in PascalCase.
/// </remarks>
public static class RouteConstants
{
    public static class HomeConstants
    {
        public const string HOME_PATH = "/home";
    }

    public static class EligibilityConstants
    {
        public const string START_PATH = "/eligibility/start";
        public const string QUESTION_ONE_PATH = "/eligibility/question-one";
        public const string QUESTION_TWO_PATH = "/eligibility/question-two";
        public const string QUESTION_THREE_PATH = "/eligibility/question-three";
        public const string QUESTION_REVIEW_PATH = "/eligibility/question-review";
        public const string ELIGIBLE_PATH = "/eligibility/eligible";
        public const string NOT_ELIGIBLE_PATH = "/eligibility/not-eligible";
    }

    public static class ApplicationConstants
    {
        public const string APPLICATION_PATH = "/application";
        public const string TASK_LIST_PATH = "/application/tasks";
    }
}
