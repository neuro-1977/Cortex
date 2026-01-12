using HtmlAgilityPack;
using Serenity.Cortex.Core.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Serenity.Cortex.Core.Services;

public sealed class CortexIngestionService
{
    private static readonly HttpClient Http = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        ConnectTimeout = TimeSpan.FromSeconds(10)
    })
    {
        Timeout = TimeSpan.FromSeconds(60) // Increased for large file downloads
    };

    public CortexIngestionService()
    {
        if (!Http.DefaultRequestHeaders.UserAgent.Any())
        {
            Http.DefaultRequestHeaders.UserAgent.ParseAdd("Serenity-Cortex/1.0");
        }
    }

    public async Task<SourceDocument> IngestAsync(string pathOrUrl, CancellationToken cancellationToken = default)
    {
        var input = (pathOrUrl ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            return new SourceDocument
            {
                Title = "(empty)",
                Type = DocumentType.Text,
                ExtractedText = string.Empty,
                IsProcessed = false,
                IncludeInContext = true
            };
        }

        if (IsHttpUrl(input, out var uri))
        {
            var text = await ExtractUrlAsync(uri!, cancellationToken).ConfigureAwait(false);
            return new SourceDocument
            {
                Title = uri!.Host,
                FilePath = input,
                Type = DocumentType.Url,
                ExtractedText = text,
                IsProcessed = LooksProcessed(text),
                IncludeInContext = true
            };
        }

        // File path.
        var fullPath = input;
        try 
        { 
            fullPath = Path.GetFullPath(input); 
        } 
        catch (Exception ex)
        {
            // If path resolution fails, use original input
            Debug.WriteLine($"[CortexIngestionService] Failed to resolve path '{input}': {ex.Message}");
        }

        var ext = Path.GetExtension(fullPath).ToLowerInvariant();
        var type = ext switch
        {
            ".pdf" => DocumentType.Pdf,
            ".md" or ".markdown" => DocumentType.Markdown,
            _ => DocumentType.Text
        };

        var title = Path.GetFileName(fullPath);

        string extracted;
        if (!File.Exists(fullPath))
        {
            extracted = "[Error: file not found]";
        }
        else if (type == DocumentType.Pdf)
        {
            extracted = ExtractPdf(fullPath);
        }
        else
        {
            extracted = await File.ReadAllTextAsync(fullPath, cancellationToken).ConfigureAwait(false);
        }

        return new SourceDocument
        {
            Title = title,
            FilePath = fullPath,
            Type = type,
            ExtractedText = extracted,
            IsProcessed = LooksProcessed(extracted),
            IncludeInContext = true
        };
    }

    private static bool IsHttpUrl(string value, out Uri? uri)
    {
        uri = null;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var u)) return false;
        if (u.Scheme != Uri.UriSchemeHttp && u.Scheme != Uri.UriSchemeHttps) return false;
        uri = u;
        return true;
    }

    private static bool LooksProcessed(string? extractedText)
    {
        if (string.IsNullOrWhiteSpace(extractedText)) return false;
        var t = extractedText.Trim();
        if (t.StartsWith("[Error", StringComparison.OrdinalIgnoreCase)) return false;
        if (t.StartsWith("Error ", StringComparison.OrdinalIgnoreCase)) return false;
        if (t.StartsWith("Unsupported", StringComparison.OrdinalIgnoreCase)) return false;
        return t.Length >= 40;
    }

    private static string ExtractPdf(string path)
    {
        var sb = new StringBuilder();
        try
        {
            using var document = PdfDocument.Open(path);
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"[PDF Error: {ex.Message}]");
        }
        return sb.ToString();
    }

    private static async Task<string> ExtractUrlAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            var html = await Http.GetStringAsync(uri, cancellationToken).ConfigureAwait(false);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove scripts/styles.
            doc.DocumentNode.Descendants()
                .Where(n => n.Name.Equals("script", StringComparison.OrdinalIgnoreCase) || n.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                .ToList()
                .ForEach(n => n.Remove());

            var text = doc.DocumentNode.InnerText ?? string.Empty;
            text = HtmlEntity.DeEntitize(text);
            text = NormalizeWhitespace(text);
            return text;
        }
        catch (Exception ex)
        {
            return $"[Web Scraping Error: {ex.Message}]";
        }
    }

    private static string NormalizeWhitespace(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var sb = new StringBuilder(s.Length);
        bool lastWasSpace = false;
        foreach (var ch in s)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!lastWasSpace)
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                }
            }
            else
            {
                sb.Append(ch);
                lastWasSpace = false;
            }
        }
        return sb.ToString().Trim();
    }
}
