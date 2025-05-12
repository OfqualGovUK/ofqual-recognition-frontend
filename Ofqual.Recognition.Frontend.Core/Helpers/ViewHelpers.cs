using System.Text.Json;

namespace Ofqual.Recognition.Frontend.Core.Helpers;

public static class ViewHelpers
{
    public static string GetAnswerValue(string? name, string? answersJson)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(answersJson))
        {
            return string.Empty;
        }

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(answersJson);

        if (dictionary != null && dictionary.TryGetValue(name, out var element))
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }

    public static List<string> GetCheckboxValues(string? name, string? answersJson)
    {
        var selectedValues = new List<string>();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(answersJson))
        {
            return selectedValues;
        }

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(answersJson);
        if (dictionary != null && dictionary.TryGetValue(name, out var element))
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            selectedValues.Add(item.GetString() ?? string.Empty);
                        }
                    }
                    break;
                case JsonValueKind.String:
                    selectedValues.Add(element.GetString() ?? string.Empty);
                    break;
            }
        }

        return selectedValues;
    }
}