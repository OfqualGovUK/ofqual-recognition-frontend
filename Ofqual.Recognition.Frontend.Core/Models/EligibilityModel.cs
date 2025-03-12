namespace Ofqual.Recognition.Frontend.Core.Models
{
    public class EligibilityModel
    {
        public required string QuestionOne { get; set; } = string.Empty;
        public required string QuestionTwo { get; set; } = string.Empty;
        public required string QuestionThree { get; set; } = string.Empty;

        public EligibilityModel() 
        { 
        
        }
    }
}