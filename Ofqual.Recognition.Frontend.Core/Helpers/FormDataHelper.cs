using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Ofqual.Recognition.Frontend.Core.Helpers;

public static class FormDataHelper
{
    public static string ConvertToJson(IFormCollection formData)
    {
        var jsonPayload = formData
            .Where(x => x.Key != "__RequestVerificationToken")
            .ToDictionary(
                x => x.Key,
                x => x.Value.Count > 1 ? x.Value.ToArray() : (object)x.Value.ToString()
            );
            
        return JsonSerializer.Serialize(jsonPayload);
    }
}