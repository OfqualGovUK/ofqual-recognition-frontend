using System.ComponentModel.DataAnnotations;

namespace Ofqual.Recognition.Frontend.Web.Models
{
    public class EligibilityModel
    {
        public required string QuestionOne { get; set; }
        public required string QuestionTwo { get; set; }
        public required string QuestionThree { get; set; }
    }
}