using Serenity.Cortex.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Serenity.Cortex.Core.Services;

/// <summary>
/// Service for managing Episode Profiles - pre-configured podcast templates.
/// </summary>
public sealed class EpisodeProfileService
{
    private static readonly string ProfilesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Serenity", "Cortex", "episode_profiles.json"
    );

    private List<EpisodeProfile>? _profiles;

    /// <summary>
    /// Get all available episode profiles.
    /// </summary>
    public async Task<List<EpisodeProfile>> GetProfilesAsync()
    {
        if (_profiles != null) return _profiles;

        // Load from file or create defaults
        if (File.Exists(ProfilesPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(ProfilesPath);
                _profiles = JsonSerializer.Deserialize<List<EpisodeProfile>>(json) ?? new List<EpisodeProfile>();
            }
            catch
            {
                _profiles = new List<EpisodeProfile>();
            }
        }
        else
        {
            _profiles = new List<EpisodeProfile>();
        }

        // If no profiles exist, create default ones
        if (_profiles.Count == 0)
        {
            _profiles = CreateDefaultProfiles();
            await SaveProfilesAsync(_profiles);
        }

        return _profiles;
    }

    /// <summary>
    /// Get a profile by name.
    /// </summary>
    public async Task<EpisodeProfile?> GetProfileByNameAsync(string name)
    {
        var profiles = await GetProfilesAsync();
        return profiles.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Save profiles to disk.
    /// </summary>
    public async Task SaveProfilesAsync(List<EpisodeProfile> profiles)
    {
        _profiles = profiles;
        var dir = Path.GetDirectoryName(ProfilesPath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(ProfilesPath, json);
    }

    /// <summary>
    /// Create default episode profiles based on open-notebook patterns.
    /// </summary>
    private static List<EpisodeProfile> CreateDefaultProfiles()
    {
        return new List<EpisodeProfile>
        {
            new EpisodeProfile
            {
                Name = "Tech Discussion",
                Description = "Technical experts with complementary perspectives. Deep-dive analysis of complex topics. Optimized for developer and technical audiences.",
                CrewMembers = new List<string> { "Dash", "Hayley" },
                DefaultBriefing = "Create a technical discussion podcast between two experts. Focus on deep analysis, practical insights, and knowledge sharing. Use natural debate and complementary perspectives.",
                NumSegments = 5,
                Format = "Tech Discussion"
            },
            new EpisodeProfile
            {
                Name = "Solo Expert",
                Description = "Single authority explaining concepts clearly. Accessible presentation style. Perfect for educational content.",
                CrewMembers = new List<string> { "Jax" },
                DefaultBriefing = "Create a solo expert podcast. The speaker should explain concepts clearly and accessibly. Use engaging delivery with rich personality.",
                NumSegments = 4,
                Format = "Solo Expert"
            },
            new EpisodeProfile
            {
                Name = "Business Analysis",
                Description = "Business-focused panel discussion. Strategic viewpoints and market analysis. Executive-level conversation style.",
                CrewMembers = new List<string> { "Jax", "Zara", "Inara" },
                DefaultBriefing = "Create a business analysis panel discussion. Focus on strategic viewpoints, market analysis, and executive-level insights. Ensure diverse perspectives on business topics.",
                NumSegments = 6,
                Format = "Business Analysis"
            },
            new EpisodeProfile
            {
                Name = "Interview Style",
                Description = "Host interviewing subject matter expert. Question-driven exploration. Broad topic coverage.",
                CrewMembers = new List<string> { "Jax", "Elias" },
                DefaultBriefing = "Create an interview-style podcast. The host should ask questions and guide the conversation. The expert should provide detailed answers and insights. Use engaging conversational format.",
                NumSegments = 5,
                Format = "Interview Style"
            },
            new EpisodeProfile
            {
                Name = "Panel Discussion",
                Description = "Multi-speaker panel with diverse perspectives. Balanced viewpoints and healthy debate.",
                CrewMembers = new List<string> { "Jax", "Zara", "Dash", "Inara", "Brock" },
                DefaultBriefing = "Create a panel discussion with multiple speakers. Ensure diverse viewpoints are represented. Use natural turn-taking and healthy debate. Bring together different expert perspectives.",
                NumSegments = 7,
                Format = "Panel Discussion"
            },
            new EpisodeProfile
            {
                Name = "Medical & Research",
                Description = "Medical and research-focused discussion. Technical accuracy with accessible explanations.",
                CrewMembers = new List<string> { "Elias", "Riven" },
                DefaultBriefing = "Create a medical and research-focused podcast. Focus on technical accuracy while maintaining accessible explanations. Use analytical and precise language.",
                NumSegments = 5,
                Format = "Medical & Research"
            }
        };
    }
}

