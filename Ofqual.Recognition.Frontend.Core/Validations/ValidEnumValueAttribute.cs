using System.ComponentModel.DataAnnotations;

namespace Ofqual.Recognition.Frontend.Core.Validations;

public class ValidEnumValueAttribute : ValidationAttribute
{
    private readonly Type _enumType;
    
    public ValidEnumValueAttribute(Type enumType)
    {
        _enumType = enumType;
        ErrorMessage = "The provided value is not valid.";
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            return true;
        }
        
        return Enum.IsDefined(_enumType, value);
    }
}