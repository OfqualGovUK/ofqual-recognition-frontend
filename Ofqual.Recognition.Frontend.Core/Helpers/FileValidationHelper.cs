using Microsoft.AspNetCore.Http;

namespace Ofqual.Recognition.Frontend.Core.Helpers;

public static class FileValidationHelper
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".csv", ".jpeg", ".jpg", ".png",
        ".xlsx", ".doc", ".docx", ".pdf",
        ".json", ".odt", ".rtf", ".txt"
    };

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/csv",
        "image/jpeg",
        "image/png",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/pdf",
        "application/json",
        "application/vnd.oasis.opendocument.text",
        "application/rtf",
        "text/plain"
    };

    public static bool IsAllowedFile(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);
        return !string.IsNullOrEmpty(extension)
               && AllowedExtensions.Contains(extension)
               && AllowedMimeTypes.Contains(file.ContentType);
    }
}