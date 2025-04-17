namespace Ofqual.Recognition.Frontend.Core.Helpers;

public static class QuestionUrlHelper
{
    public static (string taskNameUrl, string questionNameUrl)? Parse(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var segments = url.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 2)
        {
            return null;
        }

        return (segments[0], segments[1]);
    }
}