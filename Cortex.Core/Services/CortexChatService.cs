using Cortex.Core.Config;
using Cortex.Core.LLM;
using Cortex.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cortex.Core.Services;

public sealed class CortexChatService
{
    private readonly RetrievalService _retrieval = new();
    private readonly ILlmClient _llm;

    public CortexChatService(ILlmClient? llmClient = null)
    {
        _llm = llmClient ?? new LlmClient();
    }

    public async Task<CortexChatResult> ChatWithCitationsAsync(string message, IEnumerable<SourceDocument> sources, CancellationToken cancellationToken = default)
    {
        var result = new CortexChatResult();

        var safeSources = NormalizeSources(sources);
        var hits = _retrieval.Search(message, safeSources, maxHits: 6);

        // If retrieval yields nothing, we still try: either a small "best effort" grounded prompt or an honest offline message.
        if (hits.Count == 0)
        {
            var model0 = ResolveCortexModel();
            if (!CanUseModel(model0))
            {
                result.ResponseText = "I couldn't find anything relevant in the current sources. Try adding more sources or asking a more specific question.";
                return result;
            }

            var safeContext = string.Join("\n\n", safeSources.Take(2).Select(s => $"--- SOURCE: {s.Title} ---\n{s.ExtractedText}"));
            var prompt0 = $"You are a helpful assistant. Answer based on the provided sources. If the sources don't contain the answer, say you don't know.\n\nSources:\n{safeContext}\n\nQuestion: {message}\nAnswer:";

            var ans0 = await _llm.GenerateAsync(new LlmRequest(prompt0, model0), cancellationToken).ConfigureAwait(false);
            result.ResponseText = ans0;
            return result;
        }

        result.Citations = hits
            .Select(h => new CortexCitation
            {
                Rank = h.Rank,
                SourceId = h.Chunk.SourceId,
                SourceTitle = h.Chunk.SourceTitle,
                ChunkIndex = h.Chunk.ChunkIndex,
                PreviewText = TrimForPrompt(h.Chunk.Text)
            })
            .ToList();

        var passages = string.Join("\n\n", hits.Select(h => $"[{h.Rank}] ({h.Chunk.SourceTitle})\n{TrimForPrompt(h.Chunk.Text)}"));
        var sourcesList = string.Join("\n", hits.Select(h => $"[{h.Rank}] {h.Chunk.SourceTitle}"));

        var model = ResolveCortexModel();
        if (!CanUseModel(model))
        {
            result.ResponseText = $"Offline answer (grounded):\n\nMost relevant passages:\n{HitsToBullets(hits)}\n\nSources:\n{sourcesList}";
            return result;
        }

        var prompt =
            "You are a citation-aware assistant. Answer the question using ONLY the passages below.\n" +
            "Rules:\n" +
            "- If the passages don't contain the answer, say so plainly.\n" +
            "- Cite claims with bracketed citations like [1] or [2].\n" +
            "- Keep the answer concise and well-structured.\n\n" +
            $"Question: {message}\n\nPassages:\n{passages}\n\nAnswer:";

        var llmAnswer = await _llm.GenerateAsync(new LlmRequest(prompt, model), cancellationToken).ConfigureAwait(false);
        result.ResponseText = $"{llmAnswer}\n\nSources:\n{sourcesList}";
        return result;
    }

    private static List<SourceDocument> NormalizeSources(IEnumerable<SourceDocument> sources)
    {
        return (sources ?? Enumerable.Empty<SourceDocument>())
            .Where(s => s != null)
            .Where(s => s.IncludeInContext)
            .Where(s => s.IsProcessed)
            .Where(s => !string.IsNullOrWhiteSpace(s.ExtractedText) && s.ExtractedText.Trim().Length >= 40)
            .ToList();
    }

    private static string ResolveCortexModel()
    {
        var configured = CortexConfig.Get("CORTEX_MODEL");
        if (!string.IsNullOrWhiteSpace(configured)) return configured.Trim();

        if (!string.IsNullOrWhiteSpace(CortexConfig.Get("GEMINI_API_KEY"))) return "gemini-2.0-flash-exp";
        if (!string.IsNullOrWhiteSpace(CortexConfig.Get("XAI_API_KEY")) || !string.IsNullOrWhiteSpace(CortexConfig.Get("GROK_API_KEY"))) return "grok-2-latest";

        return "ollama:phi3";
    }

    private static bool CanUseModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model)) return false;
        var m = model.Trim().ToLowerInvariant();
        if (m.StartsWith("ollama:")) return true;
        if (m.Contains("gemini")) return !string.IsNullOrWhiteSpace(CortexConfig.Get("GEMINI_API_KEY"));
        if (m.StartsWith("grok") || m.Contains("xai")) return !string.IsNullOrWhiteSpace(CortexConfig.Get("XAI_API_KEY")) || !string.IsNullOrWhiteSpace(CortexConfig.Get("GROK_API_KEY"));
        return false;
    }

    private static string TrimForPrompt(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var t = text.Trim();
        return t.Length <= 900 ? t : t.Substring(0, 900) + "…";
    }

    private static string HitsToBullets(IReadOnlyList<RetrievalService.Hit> hits)
    {
        var lines = hits
            .Take(5)
            .Select(h => $"- [{h.Rank}] {h.Chunk.SourceTitle}: {TrimForPrompt(h.Chunk.Text).Replace("\n", " ")}");
        return string.Join("\n", lines);
    }
}

