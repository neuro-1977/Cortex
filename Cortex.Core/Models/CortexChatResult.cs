using System.Collections.Generic;

namespace Cortex.Core.Models;

public sealed class CortexChatResult
{
    public string? ResponseText { get; set; }
    public List<CortexCitation> Citations { get; set; } = new();
}

