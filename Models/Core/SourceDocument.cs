using System;

namespace Serenity.Cortex.Core.Models;

public enum DocumentType
{
    Text,
    Pdf,
    Markdown,
    Url,
    Youtube
}

public sealed class SourceDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? FilePath { get; set; }
    public string? Title { get; set; }
    public string? ExtractedText { get; set; }
    public string? Summary { get; set; }
    public DocumentType Type { get; set; }
    public DateTime DateAdded { get; set; } = DateTime.Now;
    public bool IsProcessed { get; set; }

    // NotebookLM-style source limiting: allow users to include/exclude a source from chat/studio context.
    public bool IncludeInContext { get; set; } = true;
}
