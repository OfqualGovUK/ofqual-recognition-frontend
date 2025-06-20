using System.Collections.Concurrent;

namespace Ofqual.Recognition.Frontend.Core.Models;

public class AttachmentCollection
{
    public ConcurrentDictionary<Guid, AttachmentDetails> Files { get; } = new();
}
