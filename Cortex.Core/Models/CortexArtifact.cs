using System;

namespace Cortex.Core.Models;

public enum ArtifactType
{
    AudioOverview,
    SlideDeck,
    VideoOverview,
    BriefingDoc,
    MindMap,
    Quiz,
    Flashcards,
    DataTable,
    Infographic
}

public sealed class CortexArtifact
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Title { get; set; }
    public string? FilePath { get; set; }
    public string? VisualPath { get; set; }
    public string? Content { get; set; }
    public ArtifactType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? Transcript { get; set; }
}

