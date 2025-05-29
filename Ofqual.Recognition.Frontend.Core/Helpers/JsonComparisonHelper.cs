using System.Text.Json;

namespace Ofqual.Recognition.Frontend.Core.Helpers;

public static class JsonComparisonHelper
{
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

        return JsonElementDeepEquals(doc1.RootElement, doc2.RootElement);
    }

    private static bool JsonElementDeepEquals(JsonElement e1, JsonElement e2)
    {
        if (e1.ValueKind != e2.ValueKind)
        {
            return false;
        }

        switch (e1.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var obj1 = e1.EnumerateObject().OrderBy(p => p.Name).ToList();
                var obj2 = e2.EnumerateObject().OrderBy(p => p.Name).ToList();

                if (obj1.Count != obj2.Count)
                {
                    return false;
                }

                for (int i = 0; i < obj1.Count; i++)
                {
                    if (obj1[i].Name != obj2[i].Name)
                    {
                        return false;
                    }
                    if (!JsonElementDeepEquals(obj1[i].Value, obj2[i].Value))
                    {
                        return false;
                    }
                }

                return true;
            }
            
            case JsonValueKind.Array:
            {
                var arr1 = e1.EnumerateArray().ToList();
                var arr2 = e2.EnumerateArray().ToList();

                if (arr1.Count != arr2.Count)
                {
                    return false;
                }

                for (int i = 0; i < arr1.Count; i++)
                {
                    if (!JsonElementDeepEquals(arr1[i], arr2[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            default:
            {
                return e1.ToString() == e2.ToString();
            }
        }
    }
}