using Cortex.Core.Helpers;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cortex.Core.Services;

public sealed class CortexVisualsService
{
    private static string GetBaseDir(string subdir)
    {
        return PathHelper.GetOutputDir(subdir);
    }

    public Task<string> CreateTextCardPngAsync(string artifactId, string title, string body)
    {
        if (string.IsNullOrWhiteSpace(artifactId)) artifactId = Guid.NewGuid().ToString();
        var baseDir = GetBaseDir("visuals");
        var path = Path.Combine(baseDir, $"{artifactId}.png");

        RenderTextCard(path, title ?? "Visual", body ?? string.Empty);
        return Task.FromResult(path);
    }

    private static void RenderTextCard(string outputPath, string title, string body)
    {
        const int width = 1280;
        const int height = 720;

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
        canvas.Clear(new SKColor(18, 18, 22));

        using var titlePaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold),
            TextSize = 52
        };

        using var bodyPaint = new SKPaint
        {
            Color = new SKColor(210, 210, 210),
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal),
            TextSize = 30
        };

        using var footerPaint = new SKPaint
        {
            Color = new SKColor(120, 180, 255),
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal),
            TextSize = 22
        };

        var marginX = 72f;
        var y = 78f;

        // Title.
        foreach (var line in WrapLines(title, titlePaint, width - 2 * (int)marginX).Take(2))
        {
            canvas.DrawText(line, marginX, y, titlePaint);
            y += titlePaint.TextSize * 1.2f;
        }

        y += 18f;

        // Body.
        var trimmedBody = (body ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        if (trimmedBody.Length > 3500) trimmedBody = trimmedBody.Substring(0, 3500) + "…";

        var availableHeight = height - 160;
        var maxLines = Math.Max(4, (int)((availableHeight - y) / (bodyPaint.TextSize * 1.25f)));

        var lines = WrapLines(trimmedBody, bodyPaint, width - 2 * (int)marginX).Take(maxLines).ToList();
        foreach (var line in lines)
        {
            canvas.DrawText(line, marginX, y, bodyPaint);
            y += bodyPaint.TextSize * 1.25f;
            if (y > height - 90) break;
        }

        // Footer.
        var footer = "Serenity • Cortex";
        canvas.DrawText(footer, marginX, height - 54, footerPaint);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? Path.GetTempPath());
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 92);
        using var fs = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        data.SaveTo(fs);
    }

    private static System.Collections.Generic.IEnumerable<string> WrapLines(string text, SKPaint paint, int maxWidth)
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

