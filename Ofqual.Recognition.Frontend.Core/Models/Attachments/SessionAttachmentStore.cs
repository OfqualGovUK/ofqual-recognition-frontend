using System.Collections.Concurrent;

namespace Ofqual.Recognition.Frontend.Core.Models;

public class SessionAttachmentStore
{
    public ConcurrentDictionary<Guid, AttachmentCollection> Links { get; } = new();
}
