using Ofqual.Recognition.Frontend.Core.Models;
using System.Collections.Concurrent;

namespace Ofqual.Recognition.Frontend.Web.Stores;

public static class AttachmentStore
{
    private static readonly ConcurrentDictionary<string, SessionAttachmentStore> _store = new();

    public static bool TryAdd(string sessionId, Guid linkId, Guid fileId, AttachmentDetails attachment)
    {
        var sessionStore = _store.GetOrAdd(sessionId, _ => new SessionAttachmentStore());
        var linkStore = sessionStore.Links.GetOrAdd(linkId, _ => new AttachmentCollection());
        return linkStore.Files.TryAdd(fileId, attachment);
    }

    public static void TryAddRange(string sessionId, Guid linkId, IEnumerable<AttachmentDetails>? attachments)
    {
        if (attachments == null)
        {
            return;
        }

        var sessionStore = _store.GetOrAdd(sessionId, _ => new SessionAttachmentStore());
        var linkStore = sessionStore.Links.GetOrAdd(linkId, _ => new AttachmentCollection());

        foreach (var attachment in attachments)
        {
            if (!IsDuplicate(sessionId, linkId, attachment.FileName, attachment.FileSize))
            {
                var fileId = Guid.NewGuid();
                linkStore.Files.TryAdd(fileId, attachment);
            }
        }
    }

    public static bool IsDuplicate(string sessionId, Guid linkId, string fileName, long fileSize)
    {
        if (!_store.TryGetValue(sessionId, out var sessionStore))
        {
            return false;
        }

        if (!sessionStore.Links.TryGetValue(linkId, out var linkStore))
        {
            return false;
        }

        return linkStore.Files.Values.Any(file =>
            file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
            file.FileSize == fileSize);
    }

    public static bool TryRemove(string sessionId, Guid linkId, Guid fileId, out AttachmentDetails? removed)
    {
        removed = null;

        if (!_store.TryGetValue(sessionId, out var sessionStore))
        {
            return false;
        }

        if (!sessionStore.Links.TryGetValue(linkId, out var linkStore))
        {
            return false;
        }

        return linkStore.Files.TryRemove(fileId, out removed);
    }

    public static bool HasAny(string sessionId, Guid linkId)
    {
        if (_store.TryGetValue(sessionId, out var sessionStore) && sessionStore.Links.TryGetValue(linkId, out var linkStore))
        {
            return !linkStore.Files.IsEmpty;
        }

        return false;
    }

    public static IReadOnlyDictionary<Guid, AttachmentDetails> GetAll(string? sessionId, Guid? linkId)
    {
        if (string.IsNullOrEmpty(sessionId) || linkId == null || linkId == Guid.Empty)
        {
            return new Dictionary<Guid, AttachmentDetails>();
        }

        if (_store.TryGetValue(sessionId, out var sessionStore) &&
            sessionStore.Links.TryGetValue(linkId.Value, out var linkStore))
        {
            return new Dictionary<Guid, AttachmentDetails>(linkStore.Files);
        }

        return new Dictionary<Guid, AttachmentDetails>();
    }

    public static void Clear(string sessionId, Guid linkId)
    {
        if (_store.TryGetValue(sessionId, out var sessionStore))
        {
            sessionStore.Links.TryRemove(linkId, out _);
        }
    }

    public static void ClearAll(string sessionId)
    {
        _store.TryRemove(sessionId, out _);
    }
}