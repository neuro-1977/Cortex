using System;
using System.Collections.Generic;

namespace Serenity.Cortex.Core.Models;

public sealed class CortexChatMessage
{
    public string? Sender { get; set; }
    public string? Message { get; set; }
    public List<CortexCitation> Citations { get; set; } = new();

    // "user" | "assistant" | "system" | "error" (kept as string for backwards compatibility)
    public string? Kind { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
}
