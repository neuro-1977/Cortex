using Serenity.Cortex.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Serenity.Cortex.Core.Persistence;

public sealed class CortexStorageService
{
    private const int CurrentVersion = 1;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private CancellationTokenSource? _debounceCts;
    private readonly object _debounceLock = new();

    public string StorePath { get; }

    public CortexStorageService(string? storePath = null)
    {
        StorePath = storePath ?? GetDefaultStorePath();
    }

    private static string GetDefaultStorePath()
    {
        var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Serenity", "Cortex");
        Directory.CreateDirectory(baseDir);
        return Path.Combine(baseDir, "cortex_projects.json");
    }

    public async Task<List<CortexProject>> LoadAsync()
    {
        try
        {
            if (!File.Exists(StorePath)) return new List<CortexProject>();

            var json = await File.ReadAllTextAsync(StorePath).ConfigureAwait(false);
            var root = JsonSerializer.Deserialize<StoreRoot>(json, JsonOptions());
            if (root?.Projects == null) return new List<CortexProject>();

            return root.Projects.Select(ToModel).ToList();
        }
        catch (Exception ex)
        {
            // Best-effort: if persistence fails, return empty rather than crashing the UI.
            Debug.WriteLine($"[CortexStorageService] Failed to load projects from {StorePath}: {ex.Message}");
            Debug.WriteLine($"[CortexStorageService] Stack trace: {ex.StackTrace}");
            return new List<CortexProject>();
        }
    }

    /// <summary>
    /// Saves projects immediately (synchronous save).
    /// </summary>
    public async Task SaveAsync(IEnumerable<CortexProject> projects)
    {
        await SaveAsyncInternal(projects).ConfigureAwait(false);
    }

    /// <summary>
    /// Saves projects with debouncing (waits 500ms before saving to batch rapid changes).
    /// </summary>
    public async Task SaveAsyncDebounced(IEnumerable<CortexProject> projects, int delayMs = 500)
    {
        lock (_debounceLock)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
        }

        var token = _debounceCts.Token;
        try
        {
            await Task.Delay(delayMs, token).ConfigureAwait(false);
            if (!token.IsCancellationRequested)
            {
                await SaveAsyncInternal(projects).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Debounce was cancelled, another save is pending
        }
    }

    /// <summary>
    /// Saves only the specified project (incremental save - faster for large project lists).
    /// </summary>
    public async Task SaveProjectAsync(CortexProject project)
    {
        try
        {
            // Load all projects
            var allProjects = await LoadAsync().ConfigureAwait(false);
            
            // Find and update the project
            var existingIndex = allProjects.FindIndex(p => p.Id == project.Id);
            if (existingIndex >= 0)
            {
                allProjects[existingIndex] = project;
            }
            else
            {
                allProjects.Add(project);
            }
            
            // Save all projects
            await SaveAsyncInternal(allProjects).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CortexStorageService] Failed to save project {project.Id}: {ex.Message}");
        }
    }

    private async Task SaveAsyncInternal(IEnumerable<CortexProject> projects)
    {
        await _saveLock.WaitAsync().ConfigureAwait(false);
        try
        {
            var root = new StoreRoot
            {
                Version = CurrentVersion,
                SavedAt = DateTimeOffset.Now,
                Projects = (projects ?? Enumerable.Empty<CortexProject>()).Select(ToDto).ToList()
            };

            // Use streaming for large files to reduce memory usage
            var json = JsonSerializer.Serialize(root, JsonOptions());
            
            // Write to temp file first, then move (atomic write)
            var tempPath = StorePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, json).ConfigureAwait(false);
            
            // Atomic move
            if (File.Exists(StorePath))
            {
                File.Replace(tempPath, StorePath, null);
            }
            else
            {
                File.Move(tempPath, StorePath);
            }
        }
        catch (Exception ex)
        {
            // Log write failures but don't crash the UI.
            Debug.WriteLine($"[CortexStorageService] Failed to save projects to {StorePath}: {ex.Message}");
            Debug.WriteLine($"[CortexStorageService] Stack trace: {ex.StackTrace}");
        }
        finally
        {
            _saveLock.Release();
        }
    }

    public async Task ExportAsync(IEnumerable<CortexProject> projects, string exportPath)
    {
        if (string.IsNullOrWhiteSpace(exportPath)) throw new ArgumentException("Export path is required.", nameof(exportPath));

        var root = new StoreRoot
        {
            Version = CurrentVersion,
            SavedAt = DateTimeOffset.Now,
            Projects = (projects ?? Enumerable.Empty<CortexProject>()).Select(ToDto).ToList()
        };

        var json = JsonSerializer.Serialize(root, JsonOptions());
        var dir = Path.GetDirectoryName(exportPath);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(exportPath, json).ConfigureAwait(false);
    }

    public async Task<List<CortexProject>> ImportAsync(string importPath)
    {
        if (string.IsNullOrWhiteSpace(importPath)) throw new ArgumentException("Import path is required.", nameof(importPath));
        if (!File.Exists(importPath)) throw new FileNotFoundException("Import file not found.", importPath);

        var json = await File.ReadAllTextAsync(importPath).ConfigureAwait(false);
        var root = JsonSerializer.Deserialize<StoreRoot>(json, JsonOptions());
        if (root?.Projects == null) return new List<CortexProject>();
        return root.Projects.Select(ToModel).ToList();
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static ProjectDto ToDto(CortexProject p)
    {
        return new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Sources = p.Sources.Select(s => new SourceDto
            {
                Id = s.Id,
                Title = s.Title,
                FilePath = s.FilePath,
                ExtractedText = s.ExtractedText,
                Summary = s.Summary,
                Type = s.Type,
                DateAdded = s.DateAdded,
                IsProcessed = s.IsProcessed,
                IncludeInContext = s.IncludeInContext
            }).ToList(),
            Artifacts = p.Artifacts.Select(a => new ArtifactDto
            {
                Id = a.Id,
                Title = a.Title,
                FilePath = a.FilePath,
                VisualPath = a.VisualPath,
                Content = a.Content,
                Type = a.Type,
                CreatedAt = a.CreatedAt,
                Transcript = a.Transcript
            }).ToList(),
            Chat = p.ChatHistory.Select(m => new ChatDto
            {
                Sender = m.Sender,
                Message = m.Message,
                Citations = (m.Citations ?? new List<CortexCitation>()).Select(c => new CitationDto
                {
                    Rank = c.Rank,
                    SourceId = c.SourceId,
                    SourceTitle = c.SourceTitle,
                    ChunkIndex = c.ChunkIndex,
                    PreviewText = c.PreviewText
                }).ToList(),
                Kind = NormalizeKind(m.Kind, m.Sender),
                Timestamp = m.Timestamp == default ? DateTimeOffset.Now : m.Timestamp
            }).ToList()
        };
    }

    private static CortexProject ToModel(ProjectDto dto)
    {
        var p = new CortexProject
        {
            Id = string.IsNullOrWhiteSpace(dto.Id) ? Guid.NewGuid().ToString() : dto.Id,
            Name = dto.Name ?? "Notebook",
        };

        foreach (var s in dto.Sources ?? new List<SourceDto>())
        {
            p.Sources.Add(new SourceDocument
            {
                Id = string.IsNullOrWhiteSpace(s.Id) ? Guid.NewGuid().ToString() : s.Id,
                Title = s.Title,
                FilePath = s.FilePath,
                ExtractedText = s.ExtractedText,
                Summary = s.Summary,
                Type = s.Type,
                DateAdded = s.DateAdded == default ? DateTime.Now : s.DateAdded,
                IsProcessed = s.IsProcessed,
                IncludeInContext = s.IncludeInContext ?? true
            });
        }

        foreach (var a in dto.Artifacts ?? new List<ArtifactDto>())
        {
            p.Artifacts.Add(new CortexArtifact
            {
                Id = string.IsNullOrWhiteSpace(a.Id) ? Guid.NewGuid().ToString() : a.Id,
                Title = a.Title,
                FilePath = a.FilePath,
                VisualPath = a.VisualPath,
                Content = a.Content,
                Type = a.Type,
                CreatedAt = a.CreatedAt == default ? DateTime.Now : a.CreatedAt,
                Transcript = a.Transcript
            });
        }

        foreach (var c in dto.Chat ?? new List<ChatDto>())
        {
            p.ChatHistory.Add(new CortexChatMessage
            {
                Sender = c.Sender ?? InferSender(c.Kind),
                Message = c.Message,
                Citations = (c.Citations ?? new List<CitationDto>()).Select(x => new CortexCitation
                {
                    Rank = x.Rank,
                    SourceId = x.SourceId,
                    SourceTitle = x.SourceTitle,
                    ChunkIndex = x.ChunkIndex,
                    PreviewText = x.PreviewText
                }).ToList(),
                Kind = NormalizeKind(c.Kind, c.Sender),
                Timestamp = c.Timestamp == default ? DateTimeOffset.Now : c.Timestamp
            });
        }

        return p;
    }

    private static string NormalizeKind(string? kind, string? sender)
    {
        var k = (kind ?? string.Empty).Trim().ToLowerInvariant();
        if (k is "user" or "assistant" or "system" or "error") return k;

        // Back-compat: infer from sender.
        if (string.IsNullOrWhiteSpace(sender)) return "system";
        if (sender.Equals("User", StringComparison.OrdinalIgnoreCase)) return "user";
        if (sender.Equals("Cortex", StringComparison.OrdinalIgnoreCase)) return "assistant";
        if (sender.Equals("Error", StringComparison.OrdinalIgnoreCase)) return "error";
        return "system";
    }

    private static string InferSender(string? kind)
    {
        return (kind ?? string.Empty).ToLowerInvariant() switch
        {
            "user" => "User",
            "assistant" => "Cortex",
            "error" => "Error",
            _ => "System"
        };
    }

    private sealed class StoreRoot
    {
        public int Version { get; set; }
        public DateTimeOffset SavedAt { get; set; }
        public List<ProjectDto> Projects { get; set; } = new();
    }

    private sealed class ProjectDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public List<SourceDto> Sources { get; set; } = new();
        public List<ArtifactDto> Artifacts { get; set; } = new();
        public List<ChatDto> Chat { get; set; } = new();
    }

    private sealed class SourceDto
    {
        public string? Id { get; set; }
        public string? FilePath { get; set; }
        public string? Title { get; set; }
        public string? ExtractedText { get; set; }
        public string? Summary { get; set; }
        public DocumentType Type { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsProcessed { get; set; }
        public bool? IncludeInContext { get; set; }
    }

    private sealed class ArtifactDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? FilePath { get; set; }
        public string? VisualPath { get; set; }
        public string? Content { get; set; }
        public ArtifactType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Transcript { get; set; }
    }

    private sealed class ChatDto
    {
        public string? Sender { get; set; }
        public string? Message { get; set; }
        public List<CitationDto> Citations { get; set; } = new();
        public string? Kind { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    private sealed class CitationDto
    {
        public int Rank { get; set; }
        public string? SourceId { get; set; }
        public string? SourceTitle { get; set; }
        public int ChunkIndex { get; set; }
        public string? PreviewText { get; set; }
    }
}
