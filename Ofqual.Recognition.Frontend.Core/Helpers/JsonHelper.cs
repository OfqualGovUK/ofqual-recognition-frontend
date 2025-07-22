using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Ofqual.Recognition.Frontend.Core.Helpers;

public static class JsonHelper
{
    public static bool IsEmptyJsonObject(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.ValueKind == JsonValueKind.Object && !doc.RootElement.EnumerateObject().Any();
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static string ConvertToJson(IFormCollection formData)
    {
        var jsonPayload = formData
            .Where(x =>
                x.Key != "__RequestVerificationToken" &&
                x.Value.Count > 0 &&
                x.Value.Any(v => !string.IsNullOrWhiteSpace(v))
            )
            .ToDictionary(
                x => x.Key,
                x => x.Value.Count > 1
                    ? x.Value.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray()
                    : (object)x.Value.ToString()
            );

        return JsonSerializer.Serialize(jsonPayload);
    }

    public static string? GetFirstAnswerFromJson(string jsonAnswer)
    {
        if (string.IsNullOrWhiteSpace(jsonAnswer))
        {
            return null;
        }

        var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonAnswer);
        if (dictionary == null || dictionary.Count == 0)
        {
            return null;
        }

        foreach (var kvp in dictionary)
        {
            var element = kvp.Value;

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var value = item.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                var value = element.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    public static bool AreEqual(string? json1, string? json2)
    {
        if (string.IsNullOrWhiteSpace(json1) && string.IsNullOrWhiteSpace(json2))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(json1) || string.IsNullOrWhiteSpace(json2))
        {
            return false;
        }

        using var doc1 = JsonDocument.Parse(json1);
        using var doc2 = JsonDocument.Parse(json2);

        return CompareElements(doc1.RootElement, doc2.RootElement);

        static bool CompareElements(JsonElement e1, JsonElement e2)
        {
            if (e1.ValueKind != e2.ValueKind)
            {
                return false;
            }

            switch (e1.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj1 = e1.EnumerateObject().OrderBy(p => p.Name).ToList();
                    var obj2 = e2.EnumerateObject().OrderBy(p => p.Name).ToList();

                    if (obj1.Count != obj2.Count)
                    {
                        return false;
                    }

                    for (int i = 0; i < obj1.Count; i++)
                    {
                        if (obj1[i].Name != obj2[i].Name ||
                            !CompareElements(obj1[i].Value, obj2[i].Value))
                        {
                            return false;
                        }
                    }

                    return true;

                case JsonValueKind.Array:
                    var arr1 = e1.EnumerateArray().ToList();
                    var arr2 = e2.EnumerateArray().ToList();

                    if (arr1.Count != arr2.Count)
                    {
                        return false;
                    }

                    for (int i = 0; i < arr1.Count; i++)
                    {
                        if (!CompareElements(arr1[i], arr2[i]))
                        {
                            return false;
                        }
                    }

                    return true;

                default:
                    return e1.ToString() == e2.ToString();
            }
        }
    }
}