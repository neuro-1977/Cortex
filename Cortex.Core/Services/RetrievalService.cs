using Cortex.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cortex.Core.Services;

public sealed class RetrievalService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a","an","the","and","or","but","if","then","else","when","while","of","to","in","on","for","from","with","as",
        "is","are","was","were","be","been","being","at","by","it","its","this","that","these","those","you","your","we","our",
        "i","me","my","they","them","their","he","him","his","she","her","hers","not","no","yes","can","could","should","would",
        "will","just","about","into","over","under","up","down","out","also","than"
    };

    private static readonly Regex WordRegex = new(@"[A-Za-z0-9][A-Za-z0-9_\-']*", RegexOptions.Compiled);

    public sealed record Chunk(string SourceId, string SourceTitle, int ChunkIndex, string Text);
    public sealed record Hit(int Rank, double Score, Chunk Chunk);

    public IReadOnlyList<Hit> Search(string query, IEnumerable<SourceDocument> sources, int maxHits = 6)
    {
        if (string.IsNullOrWhiteSpace(query) || sources == null) return Array.Empty<Hit>();

        var allChunks = BuildChunks(sources).ToList();
        if (allChunks.Count == 0) return Array.Empty<Hit>();

        var queryTerms = Tokenize(query).Distinct(StringComparer.OrdinalIgnoreCase).Where(t => !StopWords.Contains(t)).ToList();
        if (queryTerms.Count == 0) return Array.Empty<Hit>();

        // Document frequency per term across chunks.
        var df = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var term in queryTerms)
        {
            var count = 0;
            foreach (var ch in allChunks)
            {
                var set = new HashSet<string>(Tokenize(ch.Text).Where(t => !StopWords.Contains(t)), StringComparer.OrdinalIgnoreCase);
                if (set.Contains(term)) count++;
            }
            df[term] = Math.Max(1, count);
        }

        var N = allChunks.Count;
        var scored = new List<(Chunk chunk, double score)>();

        foreach (var ch in allChunks)
        {
            var tokens = Tokenize(ch.Text).Where(t => !StopWords.Contains(t)).ToList();
            if (tokens.Count == 0) continue;

            var tf = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var tok in tokens)
            {
                if (queryTerms.Contains(tok, StringComparer.OrdinalIgnoreCase))
                {
                    tf.TryGetValue(tok, out var v);
                    tf[tok] = v + 1;
                }
            }

            if (tf.Count == 0) continue;

            double score = 0;
            foreach (var term in queryTerms)
            {
                if (!tf.TryGetValue(term, out var termTf)) continue;
                var idf = Math.Log((N + 1.0) / (df[term] + 1.0)) + 1.0;
                score += (termTf / (double)tokens.Count) * idf;
            }

            if (score > 0) scored.Add((ch, score));
        }

        return scored
            .OrderByDescending(s => s.score)
            .ThenBy(s => s.chunk.SourceTitle, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(1, maxHits))
            .Select((s, idx) => new Hit(idx + 1, s.score, s.chunk))
            .ToList();
    }

    private IEnumerable<Chunk> BuildChunks(IEnumerable<SourceDocument> sources)
    {
        const int maxChunkChars = 1200;
        const int overlapChars = 200;

        foreach (var src in sources)
        {
            if (src == null) continue;
            if (!src.IsProcessed) continue;

            var text = src.ExtractedText;
            if (string.IsNullOrWhiteSpace(text)) continue;
            if (text.Trim().Length < 40) continue;

            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
            var idx = 0;
            var chunkIndex = 0;

            while (idx < normalized.Length)
            {
                var len = Math.Min(maxChunkChars, normalized.Length - idx);
                var slice = normalized.Substring(idx, len);

                var lastBreak = slice.LastIndexOf('\n');
                if (lastBreak > 300)
                {
                    slice = slice.Substring(0, lastBreak);
                    len = slice.Length;
                }

                slice = slice.Trim();
                if (slice.Length > 0)
                {
                    yield return new Chunk(src.Id, src.Title ?? "(untitled)", chunkIndex, slice);
                    chunkIndex++;
                }

                if (idx + len >= normalized.Length) break;
                idx = Math.Max(0, idx + len - overlapChars);
            }
        }
    }

    private static IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) yield break;

        foreach (Match m in WordRegex.Matches(text))
        {
            var w = m.Value.Trim().ToLowerInvariant();
            if (w.Length <= 1) continue;
            yield return w;
        }
    }
}

