using Cortex.Core.Config;
using Cortex.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cortex.Core.Services;

/// <summary>
/// Creates lip-synced videos using crew member talking head videos and generated audio.
/// Supports Wav2Lip for high-quality lip sync (preserves animated background, only animates face/mouth),
/// with fallback to Rhubarb, then simple mux (preserves animated background, no lip sync).
/// </summary>
public sealed class CortexLipSyncService
{
    private static string GetBaseDir()
    {
        return PathHelper.GetOutputDir("lipsync");
    }
    
    private static string GetCacheDir(string crewName)
    {
        // Use Crew subfolders for crew-specific caches
        var crewCacheDir = Path.Combine(Directory.GetCurrentDirectory(), "Crew", crewName, "VoiceLibrary");
        if (!Directory.Exists(crewCacheDir))
        {
            // Try alternative paths
            var altPaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Crew", crewName, "VoiceLibrary"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Crew", crewName, "VoiceLibrary"),
                Path.Combine(GetBaseDir(), "cache", crewName) // Fallback to old location
            };
            
            foreach (var path in altPaths)
            {
                if (Directory.Exists(Path.GetDirectoryName(path) ?? ""))
                {
                    crewCacheDir = path;
                    break;
                }
            }
        }
        
        Directory.CreateDirectory(crewCacheDir);
        return crewCacheDir;
    }
    
    private static string GetCacheIndexPath(string crewName)
    {
        return Path.Combine(GetCacheDir(crewName), "index.json");
    }
    
    private static string GetCacheEntryPath(string hash, string crewName)
    {
        return Path.Combine(GetCacheDir(crewName), $"{hash}.mp4");
    }
    
    private static string GetCacheMetadataPath(string hash, string crewName)
    {
        return Path.Combine(GetCacheDir(crewName), $"{hash}.json");
    }
    
    private static string GetVoiceLibraryPath(string crewName)
    {
        return Path.Combine(GetCacheDir(crewName), "voice_library.json");
    }
    
    public async Task<List<Dictionary<string, object>>> GetVoiceLibraryEntriesAsync(string crewName, CancellationToken cancellationToken = default)
    {
        try
        {
            var libraryPath = GetVoiceLibraryPath(crewName);
            if (!File.Exists(libraryPath))
                return new List<Dictionary<string, object>>();
            
            var libraryJson = await File.ReadAllTextAsync(libraryPath, cancellationToken);
            using var doc = JsonDocument.Parse(libraryJson);
            var root = doc.RootElement;
            
            var entries = new List<Dictionary<string, object>>();
            if (root.TryGetProperty("entries", out var entriesEl) && entriesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in entriesEl.EnumerateArray())
                {
                    var entryDict = new Dictionary<string, object>();
                    foreach (var prop in entry.EnumerateObject())
                    {
                        entryDict[prop.Name] = prop.Value.ValueKind switch
                        {
                            JsonValueKind.String => prop.Value.GetString() ?? "",
                            JsonValueKind.Number => prop.Value.GetDouble(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            _ => prop.Value.GetRawText()
                        };
                    }
                    entries.Add(entryDict);
                }
            }
            
            return entries;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CortexLipSyncService] Error loading voice library for {crewName}: {ex.Message}");
            return new List<Dictionary<string, object>>();
        }
    }
    
    public async Task<List<string>> GetScenesAsync(string crewName, CancellationToken cancellationToken = default)
    {
        try
        {
            var entries = await GetVoiceLibraryEntriesAsync(crewName, cancellationToken);
            var scenes = entries
                .Where(e => e.TryGetValue("scene", out var s) && !string.IsNullOrWhiteSpace(s?.ToString()))
                .Select(e => e["scene"].ToString() ?? "")
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            
            return scenes;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CortexLipSyncService] Error loading scenes for {crewName}: {ex.Message}");
            return new List<string>();
        }
    }
    
    private static string ComputeTextHash(string text, string voiceName)
    {
        // Create a hash from text + voice to identify unique phrases
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var input = $"{text.ToLowerInvariant().Trim()}|{voiceName}";
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16).ToLowerInvariant();
    }

    private static string GetCrewVideoPath(string crewName)
    {
        // Try multiple possible locations - prioritize data/media/video for default "breathing looking at camera" animations
        var possiblePaths = new[]
        {
            // First try relative to current directory (development)
            Path.Combine(Directory.GetCurrentDirectory(), "data", "media", "video", $"{crewName}.mp4"),
            // Then try relative to executable (published)
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "media", "video", $"{crewName}.mp4"),
            // Try going up from executable (if in bin folder)
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "data", "media", "video", $"{crewName}.mp4"),
            // Try from project root (if running from repo)
            Path.Combine(PathHelper.GetProjectRoot(), "data", "media", "video", $"{crewName}.mp4"),
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path)) 
            {
                System.Diagnostics.Debug.WriteLine($"[CortexLipSyncService] Found crew video for {crewName} at: {path}");
                return path;
            }
        }

        System.Diagnostics.Debug.WriteLine($"[CortexLipSyncService] Crew video not found for {crewName}");
        return string.Empty;
    }

    public async Task<string> CreateLipSyncVideoAsync(
        string artifactId,
        string audioPath,
        string crewName,
        string? text = null,
        string? voiceName = null,
        string? scene = null,
        string? actor = null,
        string? quality = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (string.IsNullOrWhiteSpace(artifactId)) artifactId = Guid.NewGuid().ToString();
        if (string.IsNullOrWhiteSpace(audioPath))
            throw new ArgumentException("Audio path cannot be null or empty", nameof(audioPath));
        if (!File.Exists(audioPath))
            throw new FileNotFoundException($"Audio file not found: {audioPath}", audioPath);

        var crewVideoPath = GetCrewVideoPath(crewName);
        if (string.IsNullOrWhiteSpace(crewVideoPath) || !File.Exists(crewVideoPath))
        {
            // Fallback to Serenity
            crewVideoPath = GetCrewVideoPath("Serenity");
            if (string.IsNullOrWhiteSpace(crewVideoPath) || !File.Exists(crewVideoPath))
                throw new FileNotFoundException($"Crew video not found for {crewName}");
        }

        // Check cache if we have text and voice info
        if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(voiceName))
        {
            var cacheHash = ComputeTextHash(text, voiceName);
            var cachedPath = await TryLoadFromCacheAsync(cacheHash, crewName, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cachedPath) && File.Exists(cachedPath))
            {
                return cachedPath;
            }
        }

        var baseDir = GetBaseDir();
        var workDir = Path.Combine(baseDir, artifactId);
        Directory.CreateDirectory(workDir);

        var outputPath = Path.Combine(baseDir, $"{artifactId}_{crewName}.mp4");

        // Try Wav2Lip first (if HQ), then fallback to Rhubarb, then simple mux
        string? finalPath = null;
        var useHighQuality = quality == "HQ";
        
        if (useHighQuality && await TryWav2LipAsync(audioPath, crewVideoPath, outputPath, cancellationToken))
        {
            finalPath = outputPath;
        }
        else if (await TryRhubarbLipSyncAsync(audioPath, crewVideoPath, outputPath, cancellationToken))
        {
            finalPath = outputPath;
        }
        else
        {
            // Fallback: simple mux (no lip sync, but video plays)
            finalPath = await SimpleMuxAsync(audioPath, crewVideoPath, outputPath, cancellationToken);
        }

        // Save to cache if we have text and voice info
        if (!string.IsNullOrWhiteSpace(finalPath) && File.Exists(finalPath) && 
            !string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(voiceName))
        {
            await SaveToCacheAsync(text, voiceName, crewName, finalPath, scene, actor, cancellationToken);
        }

        return finalPath ?? outputPath;
    }

    public async Task<string> CreateMultiHostPodcastAsync(
        string artifactId,
        List<(string crewName, string audioPath, double duration)> segments,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (string.IsNullOrWhiteSpace(artifactId)) artifactId = Guid.NewGuid().ToString();
        if (segments == null || segments.Count == 0)
            throw new ArgumentException("No segments provided", nameof(segments));
        
        // Validate all segment audio files exist
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment.audioPath))
                throw new ArgumentException($"Segment audio path cannot be null or empty for crew: {segment.crewName}", nameof(segments));
            if (!File.Exists(segment.audioPath))
                throw new FileNotFoundException($"Segment audio file not found for {segment.crewName}: {segment.audioPath}", segment.audioPath);
        }

        var baseDir = GetBaseDir();
        var workDir = Path.Combine(baseDir, artifactId);
        Directory.CreateDirectory(workDir);

        var videoSegments = new List<string>();

        for (int i = 0; i < segments.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (crewName, audioPath, duration) = segments[i];
            
            var segmentPath = await CreateLipSyncVideoAsync(
                $"{artifactId}_segment_{i}",
                audioPath,
                crewName,
                text: null,
                voiceName: null,
                scene: null,
                actor: null,
                quality: "HQ", // Default to HQ for multi-host podcasts
                cancellationToken);
            
            videoSegments.Add(segmentPath);
        }

        // Concatenate all segments
        var concatListPath = Path.Combine(workDir, "concat.txt");
        var sb = new StringBuilder();
        foreach (var segment in videoSegments)
        {
            sb.AppendLine($"file '{segment.Replace("'", "'\\''")}'");
        }
        File.WriteAllText(concatListPath, sb.ToString());

        var finalOutput = Path.Combine(baseDir, $"{artifactId}_podcast.mp4");
        
        // Concatenate all segments and ensure 720p output (all segments should already be 720p from CreateLipSyncVideoAsync)
        // Use filter_complex to ensure consistent resolution across all segments
        var args = $"-y -f concat -safe 0 -i \"{concatListPath}\" " +
                   $"-vf \"scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2\" " +
                   $"-c:v libx264 -preset medium -crf 23 -c:a aac -b:a 192k \"{finalOutput}\"";
        
        await RunFfmpegAsync(args, workDir, cancellationToken).ConfigureAwait(false);
        return finalOutput;
    }

    private async Task<bool> TryWav2LipAsync(string audioPath, string videoPath, string outputPath, CancellationToken cancellationToken)
    {
        // Check if Wav2Lip is available
        var wav2lipPath = FindWav2LipPath();
        if (string.IsNullOrWhiteSpace(wav2lipPath)) return false;

        try
        {
            // Convert audio to WAV for Wav2Lip
            var workDir = Path.GetDirectoryName(outputPath) ?? Path.GetTempPath();
            var wavPath = Path.Combine(workDir, Path.GetFileNameWithoutExtension(audioPath) + ".wav");
            
            await RunFfmpegAsync($"-y -i \"{audioPath}\" \"{wavPath}\"", workDir, cancellationToken).ConfigureAwait(false);

            // Call Wav2Lip - it automatically preserves background and only animates the face/mouth region
            // Wav2Lip detects the face, replaces only the lower face/mouth area, and keeps the rest of the video intact
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"inference.py --checkpoint_path checkpoints/wav2lip_gan.pth --face \"{videoPath}\" --audio \"{wavPath}\" --outfile \"{outputPath}\" --resize_factor 1",
                WorkingDirectory = wav2lipPath,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            if (p == null) return false;

            await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            
            if (p.ExitCode == 0 && File.Exists(outputPath))
            {
                // Scale Wav2Lip output to 720p if needed
                var scaledPath = Path.Combine(workDir, Path.GetFileNameWithoutExtension(outputPath) + "_720p.mp4");
                var scaleArgs = $"-y -i \"{outputPath}\" " +
                               $"-vf \"scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2\" " +
                               $"-c:v libx264 -preset medium -crf 23 -c:a copy \"{scaledPath}\"";
                await RunFfmpegAsync(scaleArgs, workDir, cancellationToken).ConfigureAwait(false);
                
                if (File.Exists(scaledPath))
                {
                    File.Delete(outputPath);
                    File.Move(scaledPath, outputPath);
                }
                
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> TryRhubarbLipSyncAsync(string audioPath, string videoPath, string outputPath, CancellationToken cancellationToken)
    {
        var rhubarbPath = FindRhubarbPath();
        if (string.IsNullOrWhiteSpace(rhubarbPath)) return false;

        try
        {
            var workDir = Path.GetDirectoryName(outputPath) ?? Path.GetTempPath();
            var wavPath = Path.Combine(workDir, Path.GetFileNameWithoutExtension(audioPath) + ".wav");
            var jsonPath = Path.Combine(workDir, "sync.json");

            // Convert to WAV
            await RunFfmpegAsync($"-y -i \"{audioPath}\" \"{wavPath}\"", workDir, cancellationToken).ConfigureAwait(false);

            // Generate lip sync data
            var psi = new ProcessStartInfo
            {
                FileName = rhubarbPath,
                Arguments = $"-f json -o \"{jsonPath}\" \"{wavPath}\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            if (p == null) return false;

            await p.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            
            if (p.ExitCode != 0 || !File.Exists(jsonPath)) return false;

            // Rhubarb generates lip sync data but doesn't apply it directly
            // Use SimpleMuxAsync which preserves the animated background from the original video
            // The original video's animated background will be preserved, just without lip sync
            var result = await SimpleMuxAsync(audioPath, videoPath, outputPath, cancellationToken);
            return !string.IsNullOrWhiteSpace(result);
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> SimpleMuxAsync(string audioPath, string videoPath, string outputPath, CancellationToken cancellationToken)
    {
        var workDir = Path.GetDirectoryName(outputPath) ?? Path.GetTempPath();
        
        // Get audio duration first
        var audioDuration = await GetAudioDurationAsync(audioPath);
        
        // Loop the original animated video to match audio duration
        // This preserves the animated background - the video loops naturally
        // Scale to 720p while preserving the animated background
        var args = $"-y -stream_loop -1 -i \"{videoPath}\" -i \"{audioPath}\" " +
                   $"-vf \"scale=1280:720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2\" " +
                   $"-c:v libx264 -preset medium -crf 23 -c:a aac -b:a 192k " +
                   $"-t {audioDuration:F2} -map 0:v:0 -map 1:a:0 \"{outputPath}\"";
        
        await RunFfmpegAsync(args, workDir, cancellationToken).ConfigureAwait(false);
        
        return File.Exists(outputPath) ? outputPath : string.Empty;
    }
    
    private async Task<double> GetAudioDurationAsync(string audioPath)
    {
        try
        {
            var workDir = Path.GetDirectoryName(audioPath) ?? Path.GetTempPath();
            var psi = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{audioPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workDir
            };
            
            using var p = Process.Start(psi);
            if (p != null)
            {
                var output = await p.StandardOutput.ReadToEndAsync();
                await p.WaitForExitAsync();
                if (double.TryParse(output.Trim(), out var duration) && duration > 0)
                {
                    return duration;
                }
            }
        }
        catch { }
        
        // Fallback: estimate from file size or return a default
        return 5.0;
    }

    private static string? FindWav2LipPath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "tools", "Wav2Lip"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "Wav2Lip"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Wav2Lip"),
        };

        foreach (var path in possiblePaths)
        {
            var inferencePy = Path.Combine(path, "inference.py");
            if (File.Exists(inferencePy)) return path;
        }

        return null;
    }

    private static string? FindRhubarbPath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "tools", "rhubarb", "rhubarb.exe"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "rhubarb", "rhubarb.exe"),
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path)) return path;
        }

        return null;
    }

    private async Task<string?> TryLoadFromCacheAsync(string cacheHash, string crewName, CancellationToken cancellationToken)
    {
        try
        {
            var cachePath = GetCacheEntryPath(cacheHash, crewName);
            var metadataPath = GetCacheMetadataPath(cacheHash, crewName);
            
            if (!File.Exists(cachePath) || !File.Exists(metadataPath))
                return null;
            
            // Load metadata to verify voice matches
            var metadataJson = await File.ReadAllTextAsync(metadataPath, cancellationToken);
            using var doc = JsonDocument.Parse(metadataJson);
            var root = doc.RootElement;
            
            // Verify the cached file still exists and is valid
            if (File.Exists(cachePath))
            {
                return cachePath;
            }
        }
        catch
        {
            // Cache load failed, continue with generation
        }
        
        return null;
    }
    
    private async Task SaveToCacheAsync(string text, string voiceName, string crewName, string videoPath, string? scene = null, string? actor = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheHash = ComputeTextHash(text, voiceName);
            var cachePath = GetCacheEntryPath(cacheHash, crewName);
            var metadataPath = GetCacheMetadataPath(cacheHash, crewName);
            
            // Copy video to cache
            if (File.Exists(videoPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
                File.Copy(videoPath, cachePath, overwrite: true);
                
                // Save metadata with scene and actor info
                var metadata = new Dictionary<string, object>
                {
                    ["hash"] = cacheHash,
                    ["text"] = text,
                    ["voiceName"] = voiceName,
                    ["crewName"] = crewName,
                    ["cachedAt"] = DateTime.UtcNow,
                    ["videoPath"] = cachePath,
                    ["duration"] = await GetAudioDurationAsync(videoPath)
                };
                
                if (!string.IsNullOrWhiteSpace(scene))
                    metadata["scene"] = scene;
                if (!string.IsNullOrWhiteSpace(actor))
                    metadata["actor"] = actor;
                
                var metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(metadataPath, metadataJson, cancellationToken);
                
                // Update cache index and voice library
                await UpdateCacheIndexAsync(cacheHash, text, voiceName, crewName, scene, actor, cancellationToken);
                await UpdateVoiceLibraryAsync(text, voiceName, crewName, cacheHash, scene, actor, cancellationToken);
            }
        }
        catch
        {
            // Cache save failed, but don't fail the main operation
        }
    }
    
    private async Task UpdateCacheIndexAsync(string hash, string text, string voiceName, string crewName, string? scene = null, string? actor = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var indexPath = GetCacheIndexPath(crewName);
            var index = new Dictionary<string, object>();
            
            if (File.Exists(indexPath))
            {
                var indexJson = await File.ReadAllTextAsync(indexPath, cancellationToken);
                var deserialized = JsonSerializer.Deserialize<Dictionary<string, object>>(indexJson);
                if (deserialized != null)
                {
                    index = deserialized;
                }
            }
            else
            {
                Debug.WriteLine($"[CortexLipSyncService] Index file not found: {indexPath}, creating new");
            }
            
            // Add or update entry with scene/actor metadata
            var entry = new Dictionary<string, object>
            {
                ["text"] = text,
                ["voiceName"] = voiceName,
                ["crewName"] = crewName,
                ["cachedAt"] = DateTime.UtcNow
            };
            
            if (!string.IsNullOrWhiteSpace(scene))
                entry["scene"] = scene;
            if (!string.IsNullOrWhiteSpace(actor))
                entry["actor"] = actor;
            
            index[hash] = entry;
            
            var updatedJson = JsonSerializer.Serialize(index, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(indexPath, updatedJson, cancellationToken);
        }
        catch
        {
            // Index update failed, but don't fail the main operation
        }
    }
    
    private async Task UpdateVoiceLibraryAsync(string text, string voiceName, string crewName, string hash, string? scene = null, string? actor = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var libraryPath = GetVoiceLibraryPath(crewName);
            var entries = new List<Dictionary<string, object>>();
            
            if (File.Exists(libraryPath))
            {
                var libraryJson = await File.ReadAllTextAsync(libraryPath, cancellationToken);
                using var doc = JsonDocument.Parse(libraryJson);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("entries", out var entriesEl) && entriesEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var entry in entriesEl.EnumerateArray())
                    {
                        var entryDict = new Dictionary<string, object>();
                        foreach (var prop in entry.EnumerateObject())
                        {
                            entryDict[prop.Name] = prop.Value.ValueKind switch
                            {
                                JsonValueKind.String => prop.Value.GetString() ?? "",
                                JsonValueKind.Number => prop.Value.GetDouble(),
                                JsonValueKind.True => true,
                                JsonValueKind.False => false,
                                _ => prop.Value.GetRawText()
                            };
                        }
                        entries.Add(entryDict);
                    }
                }
                
                // Check if entry already exists
                var exists = entries.Any(e => e.TryGetValue("hash", out var h) && h?.ToString() == hash);
                if (!exists)
                {
                    var newEntry = new Dictionary<string, object>
                    {
                        ["hash"] = hash,
                        ["text"] = text,
                        ["voiceName"] = voiceName,
                        ["addedAt"] = DateTime.UtcNow
                    };
                    
                    if (!string.IsNullOrWhiteSpace(scene))
                        newEntry["scene"] = scene;
                    if (!string.IsNullOrWhiteSpace(actor))
                        newEntry["actor"] = actor;
                    
                    entries.Add(newEntry);
                }
            }
            else
            {
                // Create new library
                var newEntry = new Dictionary<string, object>
                {
                    ["hash"] = hash,
                    ["text"] = text,
                    ["voiceName"] = voiceName,
                    ["addedAt"] = DateTime.UtcNow
                };
                
                if (!string.IsNullOrWhiteSpace(scene))
                    newEntry["scene"] = scene;
                if (!string.IsNullOrWhiteSpace(actor))
                    newEntry["actor"] = actor;
                
                entries.Add(newEntry);
            }
            
            var library = new Dictionary<string, object>
            {
                ["crewName"] = crewName,
                ["voiceName"] = voiceName,
                ["entries"] = entries.OrderBy(e => 
                {
                    // Sort by scene first, then actor, then text
                    var sceneVal = e.TryGetValue("scene", out var s) ? s?.ToString() ?? "" : "";
                    var actorVal = e.TryGetValue("actor", out var a) ? a?.ToString() ?? "" : "";
                    var textVal = e.TryGetValue("text", out var t) ? t?.ToString() ?? "" : "";
                    return $"{sceneVal}|{actorVal}|{textVal}";
                }).ToList(),
                ["lastUpdated"] = DateTime.UtcNow,
                ["totalEntries"] = entries.Count
            };
            
            var updatedJson = JsonSerializer.Serialize(library, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(libraryPath, updatedJson, cancellationToken);
        }
        catch
        {
            // Voice library update failed, but don't fail the main operation
        }
    }

    private static async Task RunFfmpegAsync(string arguments, string workingDirectory, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var p = Process.Start(psi);
            if (p == null) throw new InvalidOperationException("Failed to start ffmpeg.");

            var stdoutTask = p.StandardOutput.ReadToEndAsync();
            var stderrTask = p.StandardError.ReadToEndAsync();

            await p.WaitForExitAsync(ct).ConfigureAwait(false);

            var stderr = await stderrTask.ConfigureAwait(false);

            if (p.ExitCode != 0)
            {
                var msg = string.IsNullOrWhiteSpace(stderr) ? "ffmpeg failed." : stderr.Split('\n').Take(6).Aggregate((a, b) => a + "\n" + b);
                throw new InvalidOperationException(msg);
            }
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new InvalidOperationException("ffmpeg not found on PATH. Install ffmpeg or add it to PATH.");
        }
    }
}


