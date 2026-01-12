using System;
using System.Collections.Generic;

namespace Serenity.Cortex.Core.Models;

/// <summary>
/// Episode Profile - Pre-configured podcast templates for common formats.
/// Eliminates complex configuration with battle-tested combinations.
/// </summary>
public sealed class EpisodeProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    
    /// <summary>
    /// List of crew member names to use for this profile (1-5 speakers)
    /// </summary>
    public List<string> CrewMembers { get; set; } = new();
    
    /// <summary>
    /// Default briefing/prompt template for this profile
    /// </summary>
    public string DefaultBriefing { get; set; } = "";
    
    /// <summary>
    /// Number of segments for the podcast (3-20)
    /// </summary>
    public int NumSegments { get; set; } = 5;
    
    /// <summary>
    /// Format/style description (e.g., "Tech Discussion", "Interview Style")
    /// </summary>
    public string Format { get; set; } = "";
}

