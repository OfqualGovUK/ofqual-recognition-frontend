using System.ComponentModel.DataAnnotations;

namespace Ofqual.Recognition.Frontend.Web.Models
{
    public class EligibilityModel
    {
        public string QuestionOne { get; set; }
        public string QuestionTwo { get; set; }
        public string QuestionThree { get; set; }

        public bool IsEligible() => QuestionOne == "Yes" && QuestionTwo == "Yes" && QuestionThree == "Yes";
    }
}