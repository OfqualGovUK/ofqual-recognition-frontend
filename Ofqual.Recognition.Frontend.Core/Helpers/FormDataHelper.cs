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

    public static Dictionary<string, string> ConvertToDictionary(IFormCollection formData)
    {
        return formData
            .Where(kvp => kvp.Key != "__RequestVerificationToken") // Exclude the anti-forgery token
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
    }
}