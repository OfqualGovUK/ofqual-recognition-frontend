using System.ComponentModel.DataAnnotations;

namespace Ofqual.Recognition.Frontend.Web.Models
{
    public class EligibilityModel
    {
        [Required(ErrorMessage = "Please select an option for Question One.")]
        public string QuestionOne { get; set; }
        [Required(ErrorMessage = "Please select an option for Question Two.")]
        public string QuestionTwo { get; set; }
        [Required(ErrorMessage = "Please select an option for Question Three.")]
        public string QuestionThree { get; set; }

        public bool IsEligible() => QuestionOne == "Yes" && QuestionTwo == "Yes" && QuestionThree == "Yes";
    }
}