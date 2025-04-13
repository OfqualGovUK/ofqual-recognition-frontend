namespace Ofqual.Recognition.Frontend.Core.Helpers;

public static class QuestionUrlHelper
{
    public static (string taskName, string questionName)? Parse(string? url)
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