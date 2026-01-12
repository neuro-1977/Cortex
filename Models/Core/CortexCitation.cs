namespace Serenity.Cortex.Core.Models;

public sealed class CortexCitation
{
    public int Rank { get; set; }
    public string? SourceId { get; set; }
    public string? SourceTitle { get; set; }
    public int ChunkIndex { get; set; }
    public string? PreviewText { get; set; }
}
