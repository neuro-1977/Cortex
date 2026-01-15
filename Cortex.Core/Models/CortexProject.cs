using System;
using System.Collections.ObjectModel;

namespace Cortex.Core.Models;

public sealed class CortexProject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public ObservableCollection<SourceDocument> Sources { get; set; } = new();
    public ObservableCollection<CortexArtifact> Artifacts { get; set; } = new();
    public ObservableCollection<CortexChatMessage> ChatHistory { get; set; } = new();
}

