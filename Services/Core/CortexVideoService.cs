using Serenity.Cortex.Core.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Serenity.Cortex.Core.Services;

/// <summary>
/// Creates a simple MP4 by rendering slides to PNG and invoking ffmpeg.
/// This is WinUI/WinRT-free and works anywhere ffmpeg is available.
/// </summary>
public sealed class CortexVideoService
{
    private static string GetBaseDir()
    {
        return PathHelper.GetOutputDir("video");
    }

    public async Task<string> CreateMp4FromVideoOverviewJsonAsync(string artifactId, string videoOverviewJson, string? narrationAudioPath, string resolution = "720p", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(videoOverviewJson))
        {
            throw new ArgumentException("Video overview JSON cannot be null or empty", nameof(videoOverviewJson));
        }

        if (string.IsNullOrWhiteSpace(artifactId))
        {
            System.Diagnostics.Debug.WriteLine("[CortexVideoService] ArtifactId is null or empty, generating new GUID");
            artifactId = Guid.NewGuid().ToString();
        }

        System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Creating MP4 from video overview JSON for artifact {artifactId}");
        
        var baseDir = GetBaseDir();
        var workDir = Path.Combine(baseDir, artifactId);
        var slidesDir = Path.Combine(workDir, "slides");
        Directory.CreateDirectory(slidesDir);

        var slides = ParseSlides(videoOverviewJson);
        if (slides.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine($"[CortexVideoService] No slides found in JSON, adding placeholder");
            slides.Add(new Slide { Title = "Video Overview", Content = "Add sources to generate a richer video overview." });
        }
        
        System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Parsed {slides.Count} slides from JSON");

        // Render slide images.
        var slideImagePaths = new List<string>();
        for (int i = 0; i < slides.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pngPath = Path.Combine(slidesDir, $"slide_{i + 1:00}.png");
            try
            {
                RenderSlideToPng(slides[i], pngPath, i + 1, slides.Count, resolution);
                slideImagePaths.Add(pngPath);
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Rendered slide {i + 1}/{slides.Count} to {pngPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Failed to render slide {i + 1}: {ex.Message}");
                throw;
            }
        }

        // Build ffmpeg concat script.
        var secondsPerSlide = Math.Clamp(ParseDouble(CortexConfigValue("CORTEX_VIDEO_SECONDS_PER_SLIDE"), 6), 2, 20);
        var listPath = Path.Combine(workDir, "slides.txt");
        WriteConcatList(listPath, slideImagePaths, secondsPerSlide);
        System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Created concat list with {slideImagePaths.Count} slides, {secondsPerSlide}s per slide");

        var mp4Path = Path.Combine(baseDir, $"{artifactId}.mp4");

        var hasAudio = !string.IsNullOrWhiteSpace(narrationAudioPath) && File.Exists(narrationAudioPath);
        if (hasAudio)
        {
            System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Using narration audio: {narrationAudioPath}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[CortexVideoService] No narration audio provided or file not found");
        }

        // ffmpeg -y -f concat -safe 0 -i slides.txt [-i audio] -c:v libx264 -pix_fmt yuv420p [-c:a aac] -shortest out.mp4
        var args = new StringBuilder();
        args.Append("-y ");
        args.Append("-f concat -safe 0 ");
        args.Append($"-i \"{listPath}\" ");

        if (hasAudio)
        {
            args.Append($"-i \"{narrationAudioPath}\" ");
        }

        args.Append("-c:v libx264 -pix_fmt yuv420p ");

        if (hasAudio)
        {
            args.Append("-c:a aac -shortest ");
        }
        else
        {
            args.Append("-an ");
        }

        args.Append($"\"{mp4Path}\"");

        try
        {
            await RunFfmpegAsync(args.ToString(), workDir, cancellationToken).ConfigureAwait(false);
            
            if (File.Exists(mp4Path))
            {
                var fileInfo = new FileInfo(mp4Path);
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Successfully created MP4: {mp4Path} ({fileInfo.Length} bytes)");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] WARNING: MP4 file not found after ffmpeg completion: {mp4Path}");
            }
            
            return mp4Path;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Failed to create MP4: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<string> ConcatenateAudioFilesAsync(string artifactId, List<string> audioFiles, CancellationToken cancellationToken = default)
    {
        if (audioFiles == null || audioFiles.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("[CortexVideoService] ConcatenateAudioFilesAsync: No audio files provided");
            return string.Empty;
        }
        if (audioFiles.Count == 1)
        {
            var singleFile = audioFiles[0];
            if (!File.Exists(singleFile))
            {
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] ConcatenateAudioFilesAsync: Single audio file does not exist: {singleFile}");
                return string.Empty;
            }
            return singleFile;
        }

        if (string.IsNullOrWhiteSpace(artifactId))
        {
            artifactId = Guid.NewGuid().ToString();
        }

        var baseDir = GetBaseDir();
        var workDir = Path.Combine(baseDir, artifactId);
        Directory.CreateDirectory(workDir);

        // Validate all audio files exist before creating list
        foreach (var file in audioFiles)
        {
            if (!File.Exists(file))
            {
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] ConcatenateAudioFilesAsync: Audio file does not exist: {file}");
                throw new FileNotFoundException($"Audio file not found: {file}", file);
            }
        }
        
        var listPath = Path.Combine(workDir, "audio_list.txt");
        var sb = new StringBuilder();
        foreach (var file in audioFiles)
        {
            sb.AppendLine($"file '{file.Replace("'", "'\\''")}'");
        }
        File.WriteAllText(listPath, sb.ToString());

        var outPath = Path.Combine(baseDir, $"{artifactId}_combined.mp3");
        
        // ffmpeg -y -f concat -safe 0 -i audio_list.txt -c copy out.mp3
        var args = $"-y -f concat -safe 0 -i \"{listPath}\" -c copy \"{outPath}\"";

        await RunFfmpegAsync(args, workDir, cancellationToken).ConfigureAwait(false);
        return outPath;
    }
    
    public async Task<string> ConcatenateVideoFilesAsync(string artifactId, List<string> videoFiles, string resolution = "720p", CancellationToken cancellationToken = default)
    {
        if (videoFiles == null || videoFiles.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine("[CortexVideoService] ConcatenateVideoFilesAsync: No video files provided");
            return string.Empty;
        }
        if (videoFiles.Count == 1)
        {
            var singleFile = videoFiles[0];
            if (!File.Exists(singleFile))
            {
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] ConcatenateVideoFilesAsync: Single video file does not exist: {singleFile}");
                throw new FileNotFoundException($"Video file not found: {singleFile}", singleFile);
            }
            return singleFile;
        }

        if (string.IsNullOrWhiteSpace(artifactId))
        {
            artifactId = Guid.NewGuid().ToString();
        }

        var baseDir = GetBaseDir();
        var workDir = Path.Combine(baseDir, artifactId);
        Directory.CreateDirectory(workDir);

        // Validate all video files exist before creating list
        foreach (var file in videoFiles)
        {
            if (!File.Exists(file))
            {
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] ConcatenateVideoFilesAsync: Video file does not exist: {file}");
                throw new FileNotFoundException($"Video file not found: {file}", file);
            }
        }
        
        var listPath = Path.Combine(workDir, "video_list.txt");
        var sb = new StringBuilder();
        foreach (var file in videoFiles)
        {
            sb.AppendLine($"file '{file.Replace("'", "'\\''")}'");
        }
        File.WriteAllText(listPath, sb.ToString());

        var outPath = Path.Combine(baseDir, $"{artifactId}_combined.mp4");
        
        // Determine resolution based on parameter
        int width, height;
        switch (resolution.ToLowerInvariant())
        {
            case "1080p":
                width = 1920;
                height = 1080;
                break;
            case "480p":
                width = 854;
                height = 480;
                break;
            case "720p":
            default:
                width = 1280;
                height = 720;
                break;
        }
        
        // Concatenate videos and ensure all segments are normalized to target resolution
        // Use filter_complex to scale all inputs to same resolution before concatenating
        var args = $"-y -f concat -safe 0 -i \"{listPath}\" " +
                   $"-vf \"scale={width}:{height}:force_original_aspect_ratio=decrease,pad={width}:{height}:(ow-iw)/2:(oh-ih)/2\" " +
                   $"-c:v libx264 -preset medium -crf 23 -c:a aac -b:a 192k \"{outPath}\"";

        await RunFfmpegAsync(args, workDir, cancellationToken).ConfigureAwait(false);
        return outPath;
    }

    private static string? CortexConfigValue(string key)
    {
        // Avoid depending on Config namespace from here to keep layering shallow.
        return Serenity.Cortex.Core.Config.CortexConfig.Get(key);
    }

    private static double ParseDouble(string? s, double fallback)
    {
        if (string.IsNullOrWhiteSpace(s)) return fallback;
        return double.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : fallback;
    }

    private static void WriteConcatList(string path, List<string> images, double secondsPerSlide)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < images.Count; i++)
        {
            var img = images[i];
            sb.AppendLine($"file '{img.Replace("'", "'\\''")}'");
            sb.AppendLine($"duration {secondsPerSlide.ToString(CultureInfo.InvariantCulture)}");
        }

        // ffmpeg concat requires the last file repeated (or no duration line for last).
        if (images.Count > 0)
        {
            var last = images[^1];
            sb.AppendLine($"file '{last.Replace("'", "'\\''")}'");
        }

        File.WriteAllText(path, sb.ToString());
    }

    private static async Task RunFfmpegAsync(string arguments, string workingDirectory, CancellationToken ct)
    {
        const int maxRetries = 3;
        int delay = 1000;
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Running ffmpeg (attempt {attempt}/{maxRetries}): {arguments}");
                
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

                using var p = Process.Start(psi);
                if (p == null) throw new InvalidOperationException("Failed to start ffmpeg.");

                var stdoutTask = p.StandardOutput.ReadToEndAsync();
                var stderrTask = p.StandardError.ReadToEndAsync();

                await p.WaitForExitAsync(ct).ConfigureAwait(false);

                var stderr = await stderrTask.ConfigureAwait(false);
                var stdout = await stdoutTask.ConfigureAwait(false);

                if (p.ExitCode != 0)
                {
                    // Enhanced error logging for video generation debugging
                    var errorSummary = string.IsNullOrWhiteSpace(stderr) 
                        ? "ffmpeg failed with no error output." 
                        : stderr.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l) && 
                            (l.Contains("error", StringComparison.OrdinalIgnoreCase) || 
                             l.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                             l.Contains("Invalid", StringComparison.OrdinalIgnoreCase)))
                            .Take(10)
                            .Aggregate((a, b) => a + "\n" + b);
                    
                    System.Diagnostics.Debug.WriteLine($"[CortexVideoService] FFmpeg failed (exit code {p.ExitCode}): {errorSummary}");
                    System.Diagnostics.Debug.WriteLine($"[CortexVideoService] Full stderr: {stderr}");
                    
                    throw new InvalidOperationException($"Video generation failed after {attempt} attempt(s). Error: {errorSummary}. Arguments: {arguments}");
                }
                
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] FFmpeg completed successfully");
                return; // Success
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] FFmpeg not found on PATH: {ex.Message}");
                throw new InvalidOperationException($"ffmpeg not found on PATH (attempt {attempt}/{maxRetries}). Install ffmpeg or add it to PATH to generate MP4 videos. Working directory: {workingDirectory}", ex);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                lastException = ex;
                System.Diagnostics.Debug.WriteLine($"[CortexVideoService] FFmpeg attempt {attempt} failed, retrying in {delay}ms: {ex.Message}");
                await Task.Delay(delay, ct).ConfigureAwait(false);
                delay *= 2; // Exponential backoff
            }
        }

        System.Diagnostics.Debug.WriteLine($"[CortexVideoService] FFmpeg failed after {maxRetries} attempts");
        throw new InvalidOperationException($"FFmpeg operation failed after {maxRetries} attempts. Last error: {lastException?.Message ?? "Unknown error"}. Arguments: {arguments}, Working directory: {workingDirectory}", lastException);
    }

    private sealed class Slide
    {
        public string Title { get; set; } = "Slide";
        public string Content { get; set; } = string.Empty;
        public string? VisualPath { get; set; }
    }

    private static List<Slide> ParseSlides(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<Slide>();
            var trimmed = json.TrimStart();
            if (!trimmed.StartsWith("{", StringComparison.Ordinal)) return new List<Slide>();

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return new List<Slide>();

            if (!doc.RootElement.TryGetProperty("slides", out var slidesEl) || slidesEl.ValueKind != JsonValueKind.Array)
                return new List<Slide>();

            var slides = new List<Slide>();
            foreach (var s in slidesEl.EnumerateArray())
            {
                if (s.ValueKind != JsonValueKind.Object) continue;
                var title = s.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String ? t.GetString() : "Slide";
                var content = s.TryGetProperty("content", out var c) && c.ValueKind == JsonValueKind.String ? c.GetString() : string.Empty;
                var visualPath = s.TryGetProperty("visual_path", out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                
                slides.Add(new Slide { Title = title ?? "Slide", Content = content ?? string.Empty, VisualPath = visualPath });
            }

            return slides;
        }
        catch
        {
            return new List<Slide>();
        }
    }

    private static void RenderSlideToPng(Slide slide, string pngPath, int index, int total, string resolution = "720p")
    {
        // Determine resolution - default to 720p if not specified
        int width = 1280;
        int height = 720;
        if (!string.IsNullOrWhiteSpace(resolution))
        {
            switch (resolution.ToLowerInvariant())
            {
                case "1080p":
                    width = 1920;
                    height = 1080;
                    break;
                case "480p":
                    width = 854;
                    height = 480;
                    break;
                case "720p":
                default:
                    width = 1280;
                    height = 720;
                    break;
            }
        }

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        
        // 1. Draw Background
        if (!string.IsNullOrWhiteSpace(slide.VisualPath) && File.Exists(slide.VisualPath))
        {
            try
            {
                using var bgData = SKData.Create(slide.VisualPath);
                using var codec = SKCodec.Create(bgData);
                using var bitmap = SKBitmap.Decode(codec);
                canvas.DrawBitmap(bitmap, new SKRect(0, 0, width, height), new SKPaint { FilterQuality = SKFilterQuality.High });
                
                // Dim the background for readability
                canvas.DrawRect(new SKRect(0, 0, width, height), new SKPaint { Color = new SKColor(0, 0, 0, 160) });
            }
            catch
            {
                canvas.Clear(new SKColor(18, 18, 22));
            }
        }
        else
        {
            canvas.Clear(new SKColor(18, 18, 22));
        }

        using var titlePaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold),
            TextSize = 54,
            TextAlign = SKTextAlign.Left
        };

        using var bodyPaint = new SKPaint
        {
            Color = new SKColor(205, 205, 205),
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal),
            TextSize = 32
        };

        using var footerPaint = new SKPaint
        {
            Color = new SKColor(120, 180, 255),
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal),
            TextSize = 22
        };

        var marginX = 72f;
        var y = 82f;

        foreach (var line in WrapLines(slide.Title ?? "Slide", titlePaint, width - 144).Take(2))
        {
            canvas.DrawText(line, marginX, y, titlePaint);
            y += titlePaint.TextSize * 1.2f;
        }

        y += 18f;

        var body = NormalizeBody(slide.Content);
        var maxLines = 11;
        foreach (var line in WrapLines(body, bodyPaint, width - 160).Take(maxLines))
        {
            canvas.DrawText(line, marginX + 8, y, bodyPaint);
            y += bodyPaint.TextSize * 1.25f;
            if (y > height - 100) break;
        }

        var footer = $"Serenity • Cortex Video Overview   {index}/{total}";
        canvas.DrawText(footer, marginX, height - 54, footerPaint);

        Directory.CreateDirectory(Path.GetDirectoryName(pngPath) ?? Path.GetTempPath());
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 92);
        using var fs = File.Open(pngPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        data.SaveTo(fs);
    }

    private static string NormalizeBody(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;

        var lines = content
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count <= 1) return content.Trim();

        var sb = new StringBuilder();
        foreach (var l in lines.Take(8)) sb.AppendLine($"• {l.TrimStart('•', '-', '*', ' ')}");
        return sb.ToString().Trim();
    }

    private static IEnumerable<string> WrapLines(string text, SKPaint paint, int maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text)) yield break;

        foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                yield return string.Empty;
                continue;
            }

            var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var w in words)
            {
                if (sb.Length == 0)
                {
                    sb.Append(w);
                    continue;
                }

                var candidate = sb.ToString() + " " + w;
                if (paint.MeasureText(candidate) <= maxWidth)
                {
                    sb.Append(' ').Append(w);
                }
                else
                {
                    yield return sb.ToString();
                    sb.Clear();
                    sb.Append(w);
                }
            }

            if (sb.Length > 0) yield return sb.ToString();
        }
    }
}
