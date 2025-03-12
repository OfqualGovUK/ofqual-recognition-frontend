using System.ComponentModel.DataAnnotations;

namespace Ofqual.Recognition.Frontend.Core.Models
{
    public class EligibilityModel
    {
        [Required]
        public required string QuestionOne { get; set; } = string.Empty;
        [Required]
        public required string QuestionTwo { get; set; } = string.Empty;
        [Required]
        public required string QuestionThree { get; set; } = string.Empty;

        public EligibilityModel() 
        { 
        
        }
    }
}