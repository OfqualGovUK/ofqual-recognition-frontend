namespace Ofqual.Recognition.Frontend.Web.Models
{
    public class Eligibility
    {
        public string QuestionOne { get; set; }
        public string QuestionTwo { get; set; }
        public string QuestionThree { get; set; }

        public bool IsEligible() => QuestionOne == "yes" && QuestionTwo == "yes" && QuestionThree == "yes";
    }
}