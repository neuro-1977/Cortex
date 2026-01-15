using Cortex.Core.Config;
using Cortex.Core.LLM;
using Cortex.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Cortex.Core.Services;

public sealed class CortexStudioService
{
    private readonly RetrievalService _retrieval = new();
    private readonly ILlmClient _llm;
    private readonly ElevenLabsTtsService _tts = new();
    private readonly CortexVideoService _video = new();
    private readonly CortexVisualsService _visuals = new();
    private readonly CortexImageGenerationService _images = new();
    private readonly CortexLipSyncService _lipSync = new();

    public CortexStudioService(ILlmClient? llmClient = null)
    {
        _llm = llmClient ?? new LlmClient();
    }

    public Task<CortexArtifact> GenerateArtifactAsync(ArtifactType type, IEnumerable<SourceDocument> sources, CancellationToken cancellationToken = default)
        => GenerateArtifactAsync(type, sources, userInstructions: null, cancellationToken);

    public async Task<CortexArtifact> GenerateArtifactAsync(ArtifactType type, IEnumerable<SourceDocument> sources, string? userInstructions, CancellationToken cancellationToken = default)
        => await GenerateArtifactAsync(type, sources, userInstructions, videoHosts: null, useLipSync: true, cancellationToken);

    public async Task<CortexArtifact> GenerateArtifactAsync(ArtifactType type, IEnumerable<SourceDocument> sources, string? userInstructions, List<string>? videoHosts, bool useLipSync, CancellationToken cancellationToken = default)
        => await GenerateArtifactAsync(type, sources, userInstructions, videoHosts, useLipSync, audioFormat: null, audioLength: null, audioFocus: null, mindMapFormat: null, mindMapLength: null, mindMapFocus: null, cancellationToken);

    public async Task<CortexArtifact> GenerateArtifactAsync(ArtifactType type, IEnumerable<SourceDocument> sources, string? userInstructions, List<string>? videoHosts, bool useLipSync, string? audioFormat, string? audioLength, string? audioFocus, string? mindMapFormat, string? mindMapLength, string? mindMapFocus, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var safeSources = NormalizeSources(sources);
        var context = string.Join("\n\n", safeSources.Select(s => $"--- SOURCE: {s.Title} ---\n{s.ExtractedText}"));

        var model = ResolveCortexModel();

        string generatedContent;
        if (CanUseModel(model))
        {
            cancellationToken.ThrowIfCancellationRequested();
            generatedContent = await GenerateWithLlmAsync(type, context, userInstructions, model, cancellationToken).ConfigureAwait(false);
            if (LooksLikeProviderError(generatedContent))
            {
                generatedContent = GenerateOfflineFromSources(type, safeSources);
            }
        }
        else
        {
            generatedContent = GenerateOfflineFromSources(type, safeSources);
        }

        if (IsStructuredArtifact(type))
        {
            generatedContent = CleanCodeBlock(generatedContent);
            // Only fall back to placeholders if content is clearly invalid
            // Try to be lenient - if it's JSON-like, attempt to use it
            if (!IsValidStructuredArtifact(type, generatedContent))
            {
                // Last attempt: try to extract JSON from the response
                var extracted = TryExtractJsonFromText(generatedContent, type);
                if (!string.IsNullOrWhiteSpace(extracted) && IsValidStructuredArtifact(type, extracted))
                {
                    generatedContent = extracted;
                }
                else
                {
                    // Only use placeholders as last resort
                    generatedContent = GenerateOfflineFromSources(type, safeSources);
                }
            }
        }

        var artifact = new CortexArtifact
        {
            Title = $"{type} - {DateTime.Now:g}",
            Type = type,
            Content = generatedContent,
            CreatedAt = DateTime.Now
        };

        cancellationToken.ThrowIfCancellationRequested();
        
        // Post-processing into real local files (when possible).
        if (type == ArtifactType.MindMap)
        {
            // Try to generate an image from the content first
            string? imagePath = null;
            if (_images.IsConfigured() && !string.IsNullOrWhiteSpace(generatedContent))
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // Extract a visual prompt from the Mermaid code or use the content directly
                    var visualPrompt = ExtractVisualPromptFromContent(generatedContent, userInstructions);
                    if (!string.IsNullOrWhiteSpace(visualPrompt))
                    {
                        imagePath = await _images.GenerateImageAsync(artifact.Id, visualPrompt, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"[CortexStudio] Image generation failed for MindMap: {ex.Message}");
                    /* Fallback to text card if image generation fails */ 
                }
            }
            
            cancellationToken.ThrowIfCancellationRequested();
            // Use generated image if available, otherwise fallback to text card
            artifact.VisualPath = imagePath ?? await _visuals.CreateTextCardPngAsync(artifact.Id, artifact.Title ?? type.ToString(), generatedContent).ConfigureAwait(false);
        }
        else if (type == ArtifactType.Infographic)
        {
            // Process infographic as slideshow (similar to VideoOverview)
            // 1. Process visual prompts into real images
            if (_images.IsConfigured())
            {
                try
                {
                    using var doc = JsonDocument.Parse(generatedContent);
                    if (doc.RootElement.TryGetProperty("slides", out var slidesEl) && slidesEl.ValueKind == JsonValueKind.Array)
                    {
                        var newSlides = new List<object>();
                        var slideIndex = 0;
                        foreach (var slide in slidesEl.EnumerateArray())
                        {
                            var title = slide.GetProperty("title").GetString();
                            var content = slide.GetProperty("content").GetString();
                            var visualPrompt = slide.TryGetProperty("visual_prompt", out var vp) ? vp.GetString() : null;
                            string? visualPath = null;

                            if (!string.IsNullOrWhiteSpace(visualPrompt))
                            {
                                // Enhance visual prompt with style from user instructions if not already included
                                var enhancedPrompt = EnhanceVisualPromptWithStyle(visualPrompt, userInstructions);
                                visualPath = await _images.GenerateImageAsync($"{artifact.Id}_slide_{slideIndex}", enhancedPrompt, cancellationToken).ConfigureAwait(false);
                            }

                            newSlides.Add(new
                            {
                                title,
                                content,
                                visual_prompt = visualPrompt,
                                visual_path = visualPath
                            });
                            slideIndex++;
                        }

                        var narration = doc.RootElement.GetProperty("narration").GetString();
                        generatedContent = JsonSerializer.Serialize(new { narration, slides = newSlides }, new JsonSerializerOptions { WriteIndented = true });
                        artifact.Content = generatedContent;
                    }
                }
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"[CortexStudio] Image generation failed for Infographic: {ex.Message}");
                    /* Fallback to text-only if image generation fails */ 
                }
            }

            // 2. Generate audio narration
            var narrationText = TryExtractNarration(generatedContent) ?? (TryExtractTranscriptText(generatedContent) ?? generatedContent);
            artifact.Transcript = narrationText;

            string? audioPath = null;
            // Only use ElevenLabs if explicitly enabled via settings (premium service, opt-in only)
            var ttsProvider = CortexConfig.Get("TTS_PROVIDER", "Edge") ?? "Edge";
            if (ttsProvider.Equals("ElevenLabs", StringComparison.OrdinalIgnoreCase) && _tts.IsConfigured())
            {
                // Use Zara's voice for infographics (tactical officer, analytical)
                var voiceId = CortexConfig.Get("VOICE_ID_ZARA") ?? "tx9v9aCbkoEVlWzw2CyK";
                audioPath = await _tts.SynthesizeToMp3Async(artifact.Id + "_narration", narrationText, voiceId, cancellationToken).ConfigureAwait(false);
            }

            // 3. Generate video with slides and audio
            try
            {
                if (!string.IsNullOrWhiteSpace(audioPath))
                {
                    // Use default 720p resolution for video overviews (can be made configurable later)
                    artifact.FilePath = await _video.CreateMp4FromVideoOverviewJsonAsync(artifact.Id, generatedContent, audioPath, "720p", cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                artifact.FilePath = null;
                artifact.Transcript = $"{narrationText}\n\n[Video generation failed: {ex.Message}]";
            }
        }
        else if (type == ArtifactType.AudioOverview)
        {
            // Always produce valid JSON content; optionally synthesize MP3 if configured.
            var transcript = TryExtractTranscriptText(generatedContent) ?? generatedContent;
            artifact.Transcript = transcript;

            // Only use ElevenLabs if explicitly enabled via settings (premium service, opt-in only)
            var ttsProvider = CortexConfig.Get("TTS_PROVIDER", "Edge") ?? "Edge";
            if (ttsProvider.Equals("ElevenLabs", StringComparison.OrdinalIgnoreCase) && _tts.IsConfigured())
            {
                try
                {
                    // Try multi-voice synthesis if we have a JSON array of turns
                    using var doc = JsonDocument.Parse(generatedContent);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        var audioFiles = new List<string>();
                        var turnIndex = 0;
                        foreach (var turn in doc.RootElement.EnumerateArray())
                        {
                            var speaker = turn.TryGetProperty("speaker", out var sp) ? sp.GetString() : "Inara";
                            var text = turn.TryGetProperty("text", out var tx) ? tx.GetString() : "";
                            
                            if (string.IsNullOrWhiteSpace(text)) continue;

                            // Map speakers to ElevenLabs voice IDs from crew settings
                            string? voiceId = null;
                            if (speaker?.Equals("Elias", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                voiceId = CortexConfig.Get("VOICE_ID_ELIAS") ?? "8Ly6fkjN3w7ltsU7MwB5";
                            }
                            else if (speaker?.Equals("Inara", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                voiceId = CortexConfig.Get("VOICE_ID_INARA") ?? "1Ct4QEnFTKYIueJJOBtL";
                            }
                            else
                            {
                                // Default to Inara's voice
                                voiceId = CortexConfig.Get("VOICE_ID_INARA") ?? "1Ct4QEnFTKYIueJJOBtL";
                            }

                            var turnPath = await _tts.SynthesizeToMp3Async($"{artifact.Id}_turn_{turnIndex}", text, voiceId, cancellationToken).ConfigureAwait(false);
                            if (turnPath != null) audioFiles.Add(turnPath);
                            turnIndex++;
                        }

                        if (audioFiles.Count > 0)
                        {
                            artifact.FilePath = await _video.ConcatenateAudioFilesAsync(artifact.Id, audioFiles, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                catch
                {
                    // Fallback to single voice synthesis
                    var mp3 = await _tts.SynthesizeToMp3Async(artifact.Id, transcript, null, cancellationToken).ConfigureAwait(false);
                    artifact.FilePath = mp3;
                }
            }
        }
        else if (type == ArtifactType.VideoOverview)
        {
            // 1. Process visual prompts into real images
            if (_images.IsConfigured())
            {
                try
                {
                    using var doc = JsonDocument.Parse(generatedContent);
                    if (doc.RootElement.TryGetProperty("slides", out var slidesEl) && slidesEl.ValueKind == JsonValueKind.Array)
                    {
                        var newSlides = new List<object>();
                        var slideIndex = 0;
                        foreach (var slide in slidesEl.EnumerateArray())
                        {
                            var title = slide.GetProperty("title").GetString();
                            var content = slide.GetProperty("content").GetString();
                            var visualPrompt = slide.TryGetProperty("visual_prompt", out var vp) ? vp.GetString() : null;
                            string? visualPath = null;

                            if (!string.IsNullOrWhiteSpace(visualPrompt))
                            {
                                // Enhance visual prompt with style from user instructions if not already included
                                var enhancedPrompt = EnhanceVisualPromptWithStyle(visualPrompt, userInstructions);
                                visualPath = await _images.GenerateImageAsync($"{artifact.Id}_slide_{slideIndex}", enhancedPrompt, cancellationToken).ConfigureAwait(false);
                            }

                            newSlides.Add(new
                            {
                                title,
                                content,
                                visual_prompt = visualPrompt,
                                visual_path = visualPath
                            });
                            slideIndex++;
                        }

                        var narration = doc.RootElement.GetProperty("narration").GetString();
                        generatedContent = JsonSerializer.Serialize(new { narration, slides = newSlides }, new JsonSerializerOptions { WriteIndented = true });
                        artifact.Content = generatedContent;
                    }
                }
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"[CortexStudio] Image generation failed for Infographic: {ex.Message}");
                    /* Fallback to text-only if image generation fails */ 
                }
            }

            // 2. Generate MP4 if ffmpeg is present and we have audio
            var narrationText = TryExtractNarration(generatedContent) ?? (TryExtractTranscriptText(generatedContent) ?? generatedContent);
            artifact.Transcript = narrationText;

            string? audioPath = null;
            // Only use ElevenLabs if explicitly enabled via settings (premium service, opt-in only)
            var ttsProvider = CortexConfig.Get("TTS_PROVIDER", "Edge") ?? "Edge";
            if (ttsProvider.Equals("ElevenLabs", StringComparison.OrdinalIgnoreCase) && _tts.IsConfigured())
            {
                // Determine voice based on video format or default to Jax/Dash
                string? voiceId = null;
                var videoFormat = ExtractVideoFormatFromInstructions(userInstructions);
                if (videoFormat?.IndexOf("tutorial", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Tutorial format - use Dash (pilot, technical)
                    voiceId = CortexConfig.Get("VOICE_ID_DASH") ?? "dZrrLVPH4USkDvdqIvfb";
                }
                else if (videoFormat?.IndexOf("documentary", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Documentary format - use Zara (tactical officer, analytical)
                    voiceId = CortexConfig.Get("VOICE_ID_ZARA") ?? "tx9v9aCbkoEVlWzw2CyK";
                }
                else
                {
                    // Default - use Jax (Captain)
                    voiceId = CortexConfig.Get("VOICE_ID_JAX") ?? "NGCNyudVj3EecmsUXR7y";
                }
                
                audioPath = await _tts.SynthesizeToMp3Async(artifact.Id + "_narration", narrationText, voiceId, cancellationToken).ConfigureAwait(false);
            }

            try
            {
                // Use lip sync if hosts are selected and useLipSync is true
                if (useLipSync && videoHosts != null && videoHosts.Count > 0 && !string.IsNullOrWhiteSpace(audioPath))
                {
                    // For multi-host podcast, split narration and create segments
                    if (videoHosts.Count > 1)
                    {
                        // Split narration into segments for each host (simple round-robin)
                        var segments = new List<(string crewName, string audioPath, double duration)>();
                        var words = narrationText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var wordsPerHost = Math.Max(1, words.Length / videoHosts.Count);
                        
                        // For now, use single audio for all segments (can be enhanced later)
                        for (int i = 0; i < videoHosts.Count; i++)
                        {
                            var host = videoHosts[i];
                            // Estimate duration (rough: 150 words per minute)
                            var duration = wordsPerHost / 150.0 * 60.0;
                            segments.Add((host, audioPath, duration));
                        }
                        
                        artifact.FilePath = await _lipSync.CreateMultiHostPodcastAsync(artifact.Id, segments, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        // Single host lip sync
                        var host = videoHosts[0];
                        artifact.FilePath = await _lipSync.CreateLipSyncVideoAsync(artifact.Id, audioPath, host, text: null, voiceName: null, scene: null, actor: null, quality: "HQ", cancellationToken).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Fallback to regular video generation (slides with audio)
                    // Use default 720p resolution for video overviews (can be made configurable later)
                    artifact.FilePath = await _video.CreateMp4FromVideoOverviewJsonAsync(artifact.Id, generatedContent, audioPath, "720p", cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                artifact.FilePath = null;
                artifact.Transcript = $"{narrationText}\n\n[Video generation failed: {ex.Message}]";
            }
        }
        else if (type == ArtifactType.SlideDeck)
        {
            // Optionally generate visual slides if images are configured and user requested visuals
            if (_images.IsConfigured() && IncludeVisualsInInstructions(userInstructions))
            {
                try
                {
                    using var doc = JsonDocument.Parse(generatedContent);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        var newSlides = new List<object>();
                        var slideIndex = 0;
                        foreach (var slide in doc.RootElement.EnumerateArray())
                        {
                            var title = slide.GetProperty("title").GetString();
                            var content = slide.GetProperty("content").GetString();
                            
                            // Generate a visual for each slide based on its content
                            var visualPrompt = $"Create a professional slide background image for a presentation. Title: {title}. Content summary: {Truncate(content ?? "", 200)}";
                            var enhancedPrompt = EnhanceVisualPromptWithStyle(visualPrompt, userInstructions);
                            
                            string? visualPath = null;
                            try
                            {
                                visualPath = await _images.GenerateImageAsync($"{artifact.Id}_slide_{slideIndex}", enhancedPrompt, cancellationToken).ConfigureAwait(false);
                            }
                            catch { /* Continue without image if generation fails */ }

                            newSlides.Add(new
                            {
                                title,
                                content,
                                visual_path = visualPath
                            });
                            slideIndex++;
                        }
                        
                        generatedContent = JsonSerializer.Serialize(newSlides, new JsonSerializerOptions { WriteIndented = true });
                        artifact.Content = generatedContent;
                    }
                }
                catch { /* Fallback to original content if enhancement fails */ }
            }
        }

        return artifact;
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

    private async Task<string> GenerateWithLlmAsync(ArtifactType type, string context, string? userInstructions, string model, CancellationToken cancellationToken)
    {
        try
        {
            var prompt = GetPromptForType(type, Truncate(context, 16000), userInstructions);
            var result = await _llm.GenerateAsync(new LlmRequest(prompt, model), cancellationToken).ConfigureAwait(false);
            
            // Log if result looks like an error
            if (LooksLikeProviderError(result))
            {
                System.Diagnostics.Debug.WriteLine($"[CortexStudio] LLM generation returned error-like response for {type}: {result.Substring(0, Math.Min(200, result.Length))}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CortexStudio] LLM generation failed for {type}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[CortexStudio] Stack trace: {ex.StackTrace}");
            return $"Error: {ex.Message}";
        }
    }

    private static bool LooksLikeProviderError(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        var t = text.Trim();
        return t.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)
               || t.StartsWith("Exception:", StringComparison.OrdinalIgnoreCase)
               || (t.StartsWith("{", StringComparison.Ordinal) && t.IndexOf("\"error\"", StringComparison.OrdinalIgnoreCase) >= 0)
               || t.Contains("not configured", StringComparison.OrdinalIgnoreCase)
               || t.Contains("not reachable", StringComparison.OrdinalIgnoreCase)
               || t.Contains("could not connect", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidStructuredArtifact(ArtifactType type, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return false;
        var t = content.Trim();

        if (type == ArtifactType.MindMap)
        {
            return t.StartsWith("graph ", StringComparison.OrdinalIgnoreCase)
                   || t.StartsWith("mindmap", StringComparison.OrdinalIgnoreCase)
                   || t.Contains("graph TD", StringComparison.OrdinalIgnoreCase)
                   || t.Contains("graph LR", StringComparison.OrdinalIgnoreCase);
        }
        
        if (type == ArtifactType.Infographic)
        {
            if (!t.StartsWith("{", StringComparison.Ordinal)) return false;
            try
            {
                using var doc = JsonDocument.Parse(t);
                return doc.RootElement.ValueKind == JsonValueKind.Object
                       && doc.RootElement.TryGetProperty("slides", out var slides)
                       && slides.ValueKind == JsonValueKind.Array
                       && doc.RootElement.TryGetProperty("narration", out var narration)
                       && narration.ValueKind == JsonValueKind.String;
            }
            catch { return false; }
        }

        if (type is ArtifactType.Quiz or ArtifactType.Flashcards or ArtifactType.SlideDeck or ArtifactType.AudioOverview)
        {
            if (!t.StartsWith("[", StringComparison.Ordinal)) return false;
            try
            {
                using var doc = JsonDocument.Parse(t);
                return doc.RootElement.ValueKind == JsonValueKind.Array;
            }
            catch { return false; }
        }

        if (type is ArtifactType.VideoOverview)
        {
            if (!t.StartsWith("{", StringComparison.Ordinal)) return false;
            try
            {
                using var doc = JsonDocument.Parse(t);
                return doc.RootElement.ValueKind == JsonValueKind.Object
                       && doc.RootElement.TryGetProperty("slides", out var slides)
                       && slides.ValueKind == JsonValueKind.Array;
            }
            catch { return false; }
        }

        if (type == ArtifactType.DataTable)
        {
            if (!t.StartsWith("{", StringComparison.Ordinal)) return false;
            try
            {
                using var doc = JsonDocument.Parse(t);
                return doc.RootElement.ValueKind == JsonValueKind.Object
                       && doc.RootElement.TryGetProperty("columns", out var cols)
                       && cols.ValueKind == JsonValueKind.Array
                       && doc.RootElement.TryGetProperty("rows", out var rows)
                       && rows.ValueKind == JsonValueKind.Array;
            }
            catch { return false; }
        }

        return true;
    }

    private static string Truncate(string s, int maxChars)
    {
        if (string.IsNullOrEmpty(s)) return s;
        if (s.Length <= maxChars) return s;
        return s.Substring(0, maxChars) + "…";
    }

    private static bool IsStructuredArtifact(ArtifactType type)
    {
        return type == ArtifactType.MindMap
               || type == ArtifactType.Quiz
               || type == ArtifactType.Flashcards
               || type == ArtifactType.SlideDeck
               || type == ArtifactType.AudioOverview
               || type == ArtifactType.Infographic
               || type == ArtifactType.VideoOverview;
    }

    private static string CleanCodeBlock(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;
        var c = content.Trim();
        
        // Remove markdown code blocks
        if (c.StartsWith("```", StringComparison.Ordinal))
        {
            var index = c.IndexOf('\n');
            if (index != -1) c = c.Substring(index + 1);
            if (c.EndsWith("```", StringComparison.Ordinal)) c = c.Substring(0, c.Length - 3);
            c = c.Trim();
        }
        
        // Try to extract JSON from text that might have extra content
        // This is a more robust way to handle LLM output that might include conversational filler
        var jsonStart = -1;
        var jsonEnd = -1;

        // Prioritize array for quiz, flashcards, slide deck, audio overview
        if (c.Contains('[') && c.Contains(']'))
        {
            jsonStart = c.IndexOf('[');
            jsonEnd = c.LastIndexOf(']');
        }
        // Fallback to object for data table, video overview, infographic
        else if (c.Contains('{') && c.Contains('}'))
        {
            jsonStart = c.IndexOf('{');
            jsonEnd = c.LastIndexOf('}');
        }

        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            var extracted = c.Substring(jsonStart, jsonEnd - jsonStart + 1);
            // Validate it's parseable JSON
            try
            {
                JsonDocument.Parse(extracted);
                return extracted;
            }
            catch
            {
                // Not valid JSON, return original content
            }
        }
        
        return c.Trim();
    }
    
    private static string TryExtractJsonFromText(string text, ArtifactType type)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        
        var trimmed = text.Trim();
        
        // For array-based artifacts (Quiz, Flashcards, SlideDeck, AudioOverview)
        if (type is ArtifactType.Quiz or ArtifactType.Flashcards or ArtifactType.SlideDeck or ArtifactType.AudioOverview)
        {
            // Look for JSON array
            var arrayStart = trimmed.IndexOf('[');
            var arrayEnd = trimmed.LastIndexOf(']');
            if (arrayStart >= 0 && arrayEnd > arrayStart)
            {
                var extracted = trimmed.Substring(arrayStart, arrayEnd - arrayStart + 1);
                // Validate it's parseable JSON
                try
                {
                    using var doc = JsonDocument.Parse(extracted);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                        return extracted;
                }
                catch { /* Not valid JSON */ }
            }
        }
        // For object-based artifacts (DataTable, VideoOverview, Infographic)
        else if (type is ArtifactType.DataTable or ArtifactType.VideoOverview or ArtifactType.Infographic)
        {
            // Look for JSON object
            var objStart = trimmed.IndexOf('{');
            var objEnd = trimmed.LastIndexOf('}');
            if (objStart >= 0 && objEnd > objStart)
            {
                var extracted = trimmed.Substring(objStart, objEnd - objStart + 1);
                // Validate it's parseable JSON
                try
                {
                    using var doc = JsonDocument.Parse(extracted);
                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                        return extracted;
                }
                catch { /* Not valid JSON */ }
            }
        }
        
        return text; // Return original if extraction fails
    }

    private string GenerateOfflineFromSources(ArtifactType type, IEnumerable<SourceDocument> sources)
    {
        var safeSources = NormalizeSources(sources);
        var anyText = safeSources.Any(s => !string.IsNullOrWhiteSpace(s.ExtractedText));
        if (!anyText)
        {
            return type switch
            {
                ArtifactType.SlideDeck => "[]",
                ArtifactType.Quiz => "[]",
                ArtifactType.Flashcards => "[]",
                ArtifactType.VideoOverview => "{\"narration\":\"Add sources to generate a video overview.\",\"slides\":[]}",
                ArtifactType.Infographic => "{\"narration\":\"Add sources to generate an infographic.\",\"slides\":[]}",
                ArtifactType.AudioOverview => "[{\"speaker\":\"Inara\",\"text\":\"Add sources to generate an audio overview.\"}]",
                ArtifactType.DataTable => "| Item | Value |\n|---|---|\n| Status | Add sources |",
                ArtifactType.MindMap => "graph TD\n  A[Add Sources] --> B[Then generate a diagram]",
                _ => "Add sources to generate this artifact."
            };
        }

        var query = type switch
        {
            ArtifactType.MindMap => "key topics concepts relationships",
            ArtifactType.Infographic => "process steps relationships overview",
            ArtifactType.SlideDeck => "summary overview key points",
            ArtifactType.VideoOverview => "summary overview key points narrative",
            ArtifactType.AudioOverview => "summary overview key points narrative",
            ArtifactType.Quiz => "important facts definitions",
            ArtifactType.Flashcards => "terms definitions key concepts",
            ArtifactType.DataTable => "entities numbers comparisons",
            ArtifactType.BriefingDoc => "executive summary key points stakeholders timeline",
            _ => "summary key points"
        };

        var hits = _retrieval.Search(query, safeSources, maxHits: 8);
        var text = string.Join("\n\n", hits.Select(h => h.Chunk.Text));

        if (string.IsNullOrWhiteSpace(text))
        {
            text = string.Join("\n\n", safeSources
                .Take(3)
                .Select(s => $"{s.Title}\n{Truncate(s.ExtractedText ?? string.Empty, 1200)}"));
        }

        text = Truncate(text, 5000);

        return type switch
        {
            ArtifactType.SlideDeck => JsonSerializer.Serialize(BuildSlides(text)),
            ArtifactType.VideoOverview => JsonSerializer.Serialize(BuildVideoOverview(text)),
            ArtifactType.Infographic => JsonSerializer.Serialize(BuildVideoOverview(text)), // Use same format as VideoOverview
            ArtifactType.AudioOverview => JsonSerializer.Serialize(BuildAudioTurns(text)),
            ArtifactType.MindMap => BuildMermaid(text),
            ArtifactType.Flashcards => JsonSerializer.Serialize(BuildFlashcards(text)),
            ArtifactType.Quiz => JsonSerializer.Serialize(BuildQuiz(text)),
            ArtifactType.DataTable => BuildSimpleTable(text),
            ArtifactType.BriefingDoc => BuildBriefingMarkdown(text),
            _ => text
        };
    }

    private static object BuildVideoOverview(string text)
    {
        var slides = BuildSlides(text);
        var narration = BuildNarration(slides);
        var slim = slides.Select(s => new { title = s.title, content = s.content }).ToList();
        return new { narration, slides = slim };
    }

    private static List<(string title, string content)> BuildSlides(string text)
    {
        var bullets = ExtractBullets(text, max: 12);
        var slides = new List<(string title, string content)>();
        if (bullets.Count == 0)
        {
            slides.Add(("Summary", Truncate(text, 600)));
            return slides;
        }

        slides.Add(("Overview", string.Join("\n", bullets.Take(4).Select(b => $"• {b}"))));

        var remaining = bullets.Skip(4).ToList();
        var chunks = ChunkList(remaining, 3);
        var idx = 1;
        foreach (var c in chunks.Take(5))
        {
            slides.Add(($"Key Point {idx}", string.Join("\n", c.Select(b => $"• {b}"))));
            idx++;
        }

        return slides;
    }

    private static string BuildNarration(List<(string title, string content)> slides)
    {
        if (slides == null || slides.Count == 0) return "Here's a brief overview of the provided sources.";
        var sb = new StringBuilder();
        sb.AppendLine("In this overview, we'll summarize the main ideas from your sources.");
        foreach (var s in slides.Take(6))
        {
            sb.AppendLine();
            sb.AppendLine($"{s.title}. {StripBullets(s.content)}");
        }
        sb.AppendLine();
        sb.AppendLine("That's the high-level picture. Ask a question in Chat for details with citations.");
        return sb.ToString().Trim();
    }

    private static List<object> BuildAudioTurns(string text)
    {
        var bullets = ExtractBullets(text, max: 10);
        if (bullets.Count == 0) bullets.Add(Truncate(text, 300));

        var turns = new List<object>
        {
            new { speaker = "Inara", text = "Welcome to our Deep Dive analysis. We've gathered some insights from the ship's brain regarding your request." }
        };

        for (int i = 0; i < bullets.Count; i++)
        {
            var speaker = (i % 2 == 0) ? "Elias" : "Inara";
            turns.Add(new { speaker, text = bullets[i] });
        }

        turns.Add(new { speaker = "Elias", text = "If you need further clarification, just ask in the main console and we'll cross-reference the logs." });
        return turns.Cast<object>().ToList();
    }

    private static string BuildMermaid(string text)
    {
        var keywords = ExtractKeywords(text, max: 15);
        if (keywords.Count == 0) return "graph TD\n  A[Sources] --> B[No extractable keywords]";

        // Extract sentences to understand relationships
        var sentences = SplitSentences(text);
        var bullets = ExtractBullets(text, max: 12);
        
        var sb = new StringBuilder();
        sb.AppendLine("graph TD");
        
        // Use first keyword as root, or create a summary node
        var root = keywords[0];
        sb.AppendLine($"  A[\"{EscapeMermaid(root)}\"]");
        
        // Group related keywords and create meaningful connections
        var used = new HashSet<string> { root };
        var nodeMap = new Dictionary<string, string> { { root, "A" } };
        var nodeIndex = 1;
        
        // Create nodes for other important keywords
        for (int i = 1; i < keywords.Count && nodeIndex < 12; i++)
        {
            var keyword = keywords[i];
            if (used.Contains(keyword)) continue;
            
            var nodeId = $"N{nodeIndex}";
            sb.AppendLine($"  {nodeId}[\"{EscapeMermaid(keyword)}\"]");
            nodeMap[keyword] = nodeId;
            used.Add(keyword);
            nodeIndex++;
        }
        
        // Create connections based on sentence co-occurrence
        foreach (var sentence in sentences.Take(20))
        {
            var foundKeywords = keywords.Where(k => sentence.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            if (foundKeywords.Count >= 2 && nodeMap.ContainsKey(foundKeywords[0]) && nodeMap.ContainsKey(foundKeywords[1]))
            {
                var from = nodeMap[foundKeywords[0]];
                var to = nodeMap[foundKeywords[1]];
                if (from != to)
                {
                    sb.AppendLine($"  {from} --> {to}");
                }
            }
        }
        
        // If no connections were made, connect everything to root
        if (!sb.ToString().Contains("-->"))
        {
            for (int i = 1; i < nodeIndex; i++)
            {
                sb.AppendLine($"  A --> N{i}");
            }
        }
        
        return sb.ToString().Trim();
    }

    private static string EscapeMermaid(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "Item";
        return s.Replace("[", "(").Replace("]", ")").Replace("\"", "'");
    }

    private static List<object> BuildFlashcards(string text)
    {
        var keywords = ExtractKeywords(text, max: 12);
        var cards = new List<object>();
        foreach (var term in keywords)
        {
            var def = FindSentenceContaining(text, term);
            if (string.IsNullOrWhiteSpace(def)) def = $"From the sources, {term} is discussed as an important concept.";
            cards.Add(new { front = term, back = Truncate(def, 220) });
        }
        return cards;
    }

    private static List<object> BuildQuiz(string text)
    {
        var keywords = ExtractKeywords(text, max: 8);
        var questions = new List<object>();
        foreach (var term in keywords.Take(5))
        {
            var sentence = FindSentenceContaining(text, term);
            if (string.IsNullOrWhiteSpace(sentence)) continue;

            var options = new[] { term, "Not mentioned", "Unrelated detail", "Background context" };
            questions.Add(new
            {
                question = $"Which concept is described here?\n\n\"{Truncate(sentence, 180)}\"",
                options,
                answer = term
            });
        }
        return questions;
    }

    private static string BuildSimpleTable(string text)
    {
        var keywords = ExtractKeywords(text, max: 6);
        var sb = new StringBuilder();
        sb.AppendLine("| Concept | Evidence (excerpt) |");
        sb.AppendLine("|---|---|");
        foreach (var k in keywords)
        {
            var s = FindSentenceContaining(text, k);
            sb.AppendLine($"| {k.Replace("|", "\\|")} | {Truncate((s ?? string.Empty).Replace("|", "\\|"), 120)} |");
        }
        return sb.ToString().Trim();
    }

    private static string BuildBriefingMarkdown(string text)
    {
        var bullets = ExtractBullets(text, max: 10);
        var sb = new StringBuilder();
        sb.AppendLine("# Briefing");
        sb.AppendLine();
        sb.AppendLine("## Executive Summary");
        sb.AppendLine(Truncate(text, 500));
        sb.AppendLine();
        sb.AppendLine("## Key Points");
        foreach (var b in bullets.Take(8)) sb.AppendLine($"- {b}");
        sb.AppendLine();
        sb.AppendLine("## Next Questions");
        sb.AppendLine("- What are the most important claims, and which sources support them?");
        sb.AppendLine("- What are the risks/unknowns called out by the sources?");
        return sb.ToString().Trim();
    }

    private static List<string> ExtractBullets(string text, int max)
    {
        var sentences = SplitSentences(text);
        return sentences
            .Select(s => s.Trim())
            .Where(s => s.Length >= 40)
            .Select(s => s.Length > 180 ? s.Substring(0, 180).Trim() + "…" : s)
            .Distinct()
            .Take(max)
            .ToList();
    }

    private static List<string> SplitSentences(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();
        var parts = Regex.Split(text, @"(?<=[\.!\?])\s+");
        return parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
    }

    private static List<string> ExtractKeywords(string text, int max)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();
        var stop = new HashSet<string>(new[]
        {
            "the","and","for","with","that","this","from","into","your","their","they","them","then","than","have","has","had",
            "are","was","were","be","been","being","will","would","can","could","should","may","might","not","but","you","our",
            "also","more","most","some","such","use","using","used","over","under","between","within","without","about","into","onto",
            "what","when","where","why","how","which","who","whom","because","while","during"
        }, StringComparer.OrdinalIgnoreCase);

        var matches = Regex.Matches(text, @"[A-Za-z][A-Za-z\-]{2,}");
        var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in matches)
        {
            var w = m.Value.Trim();
            if (stop.Contains(w)) continue;
            if (w.Length < 3) continue;
            freq[w] = freq.TryGetValue(w, out var c) ? c + 1 : 1;
        }

        return freq
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Select(kv => ToTitleCase(kv.Key))
            .Distinct()
            .Take(max)
            .ToList();
    }

    private static string? FindSentenceContaining(string text, string term)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term)) return null;
        var sentences = SplitSentences(text);
        return sentences.FirstOrDefault(s => s.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static string StripBullets(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;
        return content.Replace("•", string.Empty).Replace("\n", " ").Trim();
    }

    private static List<List<T>> ChunkList<T>(List<T> list, int chunkSize)
    {
        var chunks = new List<List<T>>();
        for (int i = 0; i < list.Count; i += chunkSize)
        {
            chunks.Add(list.Skip(i).Take(chunkSize).ToList());
        }
        return chunks;
    }

    private static string ToTitleCase(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        if (s.Length == 1) return s.ToUpperInvariant();
        return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
    }

    private static string? TryExtractNarration(string content)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            var trimmed = content.TrimStart();
            if (!trimmed.StartsWith("{", StringComparison.Ordinal)) return null;

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;
            if (doc.RootElement.TryGetProperty("narration", out var nar) && nar.ValueKind == JsonValueKind.String)
            {
                var s = nar.GetString();
                return string.IsNullOrWhiteSpace(s) ? null : s;
            }
        }
        catch { }
        return null;
    }

    private static string? TryExtractTranscriptText(string content)
    {
        // If content is JSON transcript: [ { speaker, text }, ... ]
        try
        {
            if (string.IsNullOrWhiteSpace(content)) return null;
            var trimmed = content.TrimStart();
            if (!trimmed.StartsWith("[", StringComparison.Ordinal)) return null;

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return null;

            using var sw = new StringWriter();
            foreach (var turn in doc.RootElement.EnumerateArray())
            {
                var speaker = turn.TryGetProperty("speaker", out var sp) ? sp.GetString() : null;
                var text = turn.TryGetProperty("text", out var tx) ? tx.GetString() : null;
                if (string.IsNullOrWhiteSpace(text)) continue;

                if (!string.IsNullOrWhiteSpace(speaker)) sw.WriteLine($"{speaker}: {text}");
                else sw.WriteLine(text);

                sw.WriteLine();
            }

            var result = sw.ToString().Trim();
            return string.IsNullOrWhiteSpace(result) ? null : result;
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractVisualPromptFromContent(string content, string? userInstructions = null)
    {
        if (string.IsNullOrWhiteSpace(content)) return "";
        
        // Extract visual style from user instructions if present
        var visualStyle = ExtractVisualStyleFromInstructions(userInstructions);
        
        // If it's Mermaid code, extract meaningful elements and create a rich visual description
        if (content.Contains("graph") || content.Contains("mindmap"))
        {
            var nodes = new HashSet<string>();
            var relationships = new List<string>();
            
            // Extract node labels: A[Label] or A{Label}
            var nodePattern = @"\[([^\]]+)\]|\{([^\}]+)\}";
            var matches = Regex.Matches(content, nodePattern);
            foreach (Match match in matches)
            {
                var label = match.Groups[1].Value ?? match.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(label) && label.Length > 1)
                    nodes.Add(label.Trim());
            }
            
            // Extract relationships: A --> B
            var relPattern = @"(\w+)\s*--[->]+\s*(\w+)";
            var relMatches = Regex.Matches(content, relPattern);
            foreach (Match match in relMatches)
            {
                var from = match.Groups[1].Value;
                var to = match.Groups[2].Value;
                if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
                    relationships.Add($"{from} connected to {to}");
            }
            
            var nodeList = string.Join(", ", nodes.Take(15));
            var relList = string.Join(", ", relationships.Take(8));
            
            if (string.IsNullOrWhiteSpace(nodeList)) return "";
            
            // Create a rich, descriptive prompt for image generation
            var prompt = $"Create a professional, visually appealing infographic diagram showing: {nodeList}";
            if (!string.IsNullOrWhiteSpace(relList))
            {
                prompt += $". The diagram should illustrate the relationships: {relList}";
            }
            
            // Add visual style if specified
            if (!string.IsNullOrWhiteSpace(visualStyle))
            {
                prompt += $". Style: {visualStyle}";
            }
            else
            {
                prompt += ". Use modern design principles with clear typography, icons, and color coding to make it informative and visually engaging.";
            }
            
            return prompt;
        }
        
        // For other content, create a comprehensive summary prompt
        var truncated = Truncate(content, 400);
        var basePrompt = $"Create a professional infographic visualization that summarizes and presents the following information in a clear, visually appealing way: {truncated}";
        
        if (!string.IsNullOrWhiteSpace(visualStyle))
        {
            return $"{basePrompt}. Style: {visualStyle}";
        }
        
        return $"{basePrompt}. Use modern design with icons, charts, and clear typography.";
    }
    
    private static string? ExtractVisualStyleFromInstructions(string? instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions)) return null;
        
        // Look for "Style:" or "Visual Style:" in instructions
        var stylePattern = @"(?:Style|Visual Style):\s*([^\.]+)";
        var match = Regex.Match(instructions, stylePattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
        
        // Check for common style keywords
        var styles = new[] { "Classic", "Whiteboard", "Watercolor", "Retro Print", "Heritage", "Paper-craft", "Kawaii", "Anime", "Cyberpunk", "Cinematic", "3D Render", "Sketch" };
        foreach (var style in styles)
        {
            if (instructions.IndexOf(style, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return style;
            }
        }
        
        return null;
    }
    
    private static string EnhanceVisualPromptWithStyle(string prompt, string? userInstructions)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return prompt ?? "";
        
        var style = ExtractVisualStyleFromInstructions(userInstructions);
        if (string.IsNullOrWhiteSpace(style)) return prompt;
        
        // Check if style is already mentioned in the prompt
        if (prompt.IndexOf(style, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return prompt;
        }
        
        // Append style to the prompt
        return $"{prompt}. Visual style: {style}";
    }
    
    private static string? ExtractVideoFormatFromInstructions(string? instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions)) return null;
        
        // Look for "Video Format:" or "Format:" in instructions
        var formatPattern = @"(?:Video\s+)?Format:\s*([^\.]+)";
        var match = Regex.Match(instructions, formatPattern, RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }
        
        // Check for common format keywords
        var formats = new[] { "Brief", "Detailed", "Tutorial", "Documentary" };
        foreach (var format in formats)
        {
            if (instructions.IndexOf(format, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return format;
            }
        }
        
        return null;
    }
    
    private static bool IncludeVisualsInInstructions(string? instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions)) return false;
        
        // Check for explicit visual requests
        if (instructions.IndexOf("include visuals", StringComparison.OrdinalIgnoreCase) >= 0 ||
            instructions.IndexOf("with visuals", StringComparison.OrdinalIgnoreCase) >= 0 ||
            instructions.IndexOf("visual style", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }
        
        return false;
    }

    private static string BuildMindMapPrompt(string extra, string context, string? userInstructions)
    {
        var format = "Deep Dive";
        var length = "Default";
        var focus = string.Empty;
        
        // Extract format, length, and focus from user instructions
        if (!string.IsNullOrWhiteSpace(userInstructions))
        {
            if (userInstructions.Contains("Format:", StringComparison.OrdinalIgnoreCase))
            {
                var formatMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Format:\s*([^\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (formatMatch.Success) format = formatMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Length:", StringComparison.OrdinalIgnoreCase))
            {
                var lengthMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Length:\s*([^\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (lengthMatch.Success) length = lengthMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Focus:", StringComparison.OrdinalIgnoreCase))
            {
                var focusMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Focus:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (focusMatch.Success) focus = focusMatch.Groups[1].Value.Trim();
            }
        }
        
        var formatDesc = format switch
        {
            "Brief" => "A concise overview focusing on core ideas and main connections.",
            "Critique" => "An analytical review highlighting strengths, gaps, and critical relationships.",
            "Debate" => "A balanced view showing multiple perspectives and contrasting viewpoints.",
            _ => "A comprehensive exploration connecting all major themes, subtopics, and relationships."
        };
        
        var lengthDesc = length switch
        {
            "Short" => "Keep it focused with 10-20 key nodes.",
            "Long" => "Go deep with 30-50 nodes showing extensive connections.",
            _ => "Balanced with 20-30 nodes covering main topics and relationships."
        };
        
        var focusText = string.IsNullOrWhiteSpace(focus) ? string.Empty : $"\n\nFocus specifically on: {focus}";
        
        // Leverage Gemini's graph-building capabilities: ask for explicit graph structure
        return $"Using graph algorithms and entity extraction, analyze the provided source content and build a knowledge graph mind map. {formatDesc} {lengthDesc}{focusText}\n\n" +
               $"Employ entity extraction (identify key nouns, concepts, people, places, things) and relationship mapping (is-a, has-a, part-of, relates-to, causes) to construct a graph structure.\n\n" +
               $"Generate a Mermaid.js mind map or graph that:\n" +
               $"1. Identifies the main central topic or theme (center node) - this is the root of your graph\n" +
               $"2. Uses entity extraction to find all key concepts, creating nodes for each\n" +
               $"3. Maps relationships between entities, creating labeled edges (e.g., 'Mal' -> 'captain of' -> 'Serenity')\n" +
               $"4. Clusters related concepts together using hierarchical relationships\n" +
               $"5. Shows major branches and sub-topics radiating outward from the center\n" +
               $"6. Illustrates connections between concepts with meaningful edge labels\n" +
               $"7. Creates a meaningful visual representation that organizes the information hierarchically\n" +
               $"8. Each node should represent a key concept, entity, or idea from the sources\n" +
               $"9. Edges should show explicit relationships with labels (e.g., 'is part of', 'relates to', 'causes', 'depends on')\n\n" +
               $"Think like a graph database: extract entities as nodes, map relationships as edges. Return ONLY the mermaid code (no markdown, no explanations). Make it detailed and informative with clear node labels and edge relationships.{extra}\n\nContext:\n{context}";
    }
    
    private static string BuildFlashCardsPrompt(string extra, string context, string? userInstructions)
    {
        var numCards = 10;
        var focus = string.Empty;
        
        // Extract flash card settings from user instructions
        if (!string.IsNullOrWhiteSpace(userInstructions))
        {
            if (userInstructions.Contains("Number:", StringComparison.OrdinalIgnoreCase) || userInstructions.Contains("Number of Cards:", StringComparison.OrdinalIgnoreCase))
            {
                var numMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Number(?:\s+of\s+Cards)?:\s*([^\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (numMatch.Success)
                {
                    var numStr = numMatch.Groups[1].Value.Trim();
                    // Handle text values
                    if (numStr.Equals("Fewer", StringComparison.OrdinalIgnoreCase))
                        numCards = 8;
                    else if (numStr.Equals("Standard", StringComparison.OrdinalIgnoreCase))
                        numCards = 12;
                    else if (numStr.Equals("More", StringComparison.OrdinalIgnoreCase))
                        numCards = 20;
                    else if (int.TryParse(numStr, out var num))
                        numCards = Math.Clamp(num, 1, 50);
                }
            }
            if (userInstructions.Contains("Description:", StringComparison.OrdinalIgnoreCase))
            {
                var descMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Description:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (descMatch.Success) focus = descMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Focus:", StringComparison.OrdinalIgnoreCase))
            {
                var focusMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Focus:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (focusMatch.Success) focus = focusMatch.Groups[1].Value.Trim();
            }
        }
        
        var focusText = string.IsNullOrWhiteSpace(focus) ? string.Empty : $"\n\nFocus specifically on: {focus}";
        
        return $"Generate flash cards JSON array from the provided sources. Number of cards: {numCards}.{focusText}\n\n" +
               $"Format each card as: {{'front': '...', 'back': '...', 'explanation': '...', 'source_link': '...'}}\n\n" +
               $"The front should be a question, term, or concept. The back should be the answer, definition, or explanation. " +
               $"Include a brief explanation and a source_link pointing to the relevant section of the source material.\n\n" +
               $"Return ONLY a JSON array with {numCards} flash cards.{extra}\n\nContext:\n{context}";
    }
    
    private static string BuildSlideDeckPrompt(string extra, string context, string? userInstructions)
    {
        var format = "Presentation";
        var length = "Default";
        var numberOfSlides = 5;
        var visualStyle = "Classic";
        var focus = string.Empty;
        
        // Extract slide deck settings from user instructions
        if (!string.IsNullOrWhiteSpace(userInstructions))
        {
            if (userInstructions.Contains("Format:", StringComparison.OrdinalIgnoreCase))
            {
                var formatMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Format:\s*([^\.]+?)(?:\n|\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (formatMatch.Success) format = formatMatch.Groups[1].Value.Trim();
            }
            
            if (userInstructions.Contains("Length:", StringComparison.OrdinalIgnoreCase))
            {
                var lengthMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Length:\s*([^\.]+?)(?:\n|\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (lengthMatch.Success) length = lengthMatch.Groups[1].Value.Trim();
            }
            
            if (userInstructions.Contains("Number of Slides:", StringComparison.OrdinalIgnoreCase))
            {
                var numMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Number of Slides:\s*(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (numMatch.Success && int.TryParse(numMatch.Groups[1].Value, out var num))
                    numberOfSlides = Math.Clamp(num, 1, 20);
            }
            
            if (userInstructions.Contains("Visual Style:", StringComparison.OrdinalIgnoreCase))
            {
                var styleMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Visual Style:\s*([^\.]+?)(?:\n|\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (styleMatch.Success) visualStyle = styleMatch.Groups[1].Value.Trim();
            }
            
            if (userInstructions.Contains("Focus:", StringComparison.OrdinalIgnoreCase))
            {
                var focusMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Focus:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (focusMatch.Success) focus = focusMatch.Groups[1].Value.Trim();
            }
        }
        
        var formatText = format switch
        {
            "Summary" => "Create a concise summary presentation",
            "Tutorial" => "Create a step-by-step tutorial presentation",
            "Executive" => "Create an executive-level strategic presentation",
            _ => "Create a standard presentation"
        };
        
        var focusText = string.IsNullOrWhiteSpace(focus) ? string.Empty : $"\n\nFocus specifically on: {focus}";
        var visualStyleText = visualStyle != "Classic" ? $" Use a {visualStyle.ToLower()} visual style." : "";
        
        return $"Based on the following text, generate a {formatText.ToLower()} slide deck as a JSON array. " +
               $"Format: [{{'title': '...', 'content': '...'}}]. " +
               $"Create exactly {numberOfSlides} slides. " +
               $"Each slide should be clear, focused, and well-structured.{visualStyleText} " +
               $"If visuals are requested, the content should be suitable for generating background images in a {visualStyle.ToLower()} style. " +
               $"Return ONLY the JSON.{focusText}{extra}\n\nContext:\n{context}";
    }
    
    private static string BuildBriefingDocPrompt(string extra, string context, string? userInstructions)
    {
        var format = "Comprehensive";
        var length = "Default";
        var tone = "Formal";
        var focus = string.Empty;
        
        // Extract briefing doc settings from user instructions
        if (!string.IsNullOrWhiteSpace(userInstructions))
        {
            if (userInstructions.Contains("Format:", StringComparison.OrdinalIgnoreCase))
            {
                var formatMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Format:\s*([^\.]+?)(?:\n|\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (formatMatch.Success) format = formatMatch.Groups[1].Value.Trim();
            }
            
            if (userInstructions.Contains("Length:", StringComparison.OrdinalIgnoreCase))
            {
                var lengthMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Length:\s*([^\.]+?)(?:\n|\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (lengthMatch.Success) length = lengthMatch.Groups[1].Value.Trim();
            }
            
            if (userInstructions.Contains("Tone:", StringComparison.OrdinalIgnoreCase))
            {
                var toneMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Tone:\s*([^\.]+?)(?:\n|\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (toneMatch.Success) tone = toneMatch.Groups[1].Value.Trim();
            }
            
            if (userInstructions.Contains("Focus:", StringComparison.OrdinalIgnoreCase))
            {
                var focusMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Focus:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (focusMatch.Success) focus = focusMatch.Groups[1].Value.Trim();
            }
        }
        
        var formatText = format switch
        {
            "Executive Summary" => "executive summary format",
            "Tactical" => "tactical briefing format",
            "Timeline" => "chronological timeline format",
            _ => "comprehensive briefing format"
        };
        
        var lengthText = length switch
        {
            "Short" => "brief (2-3 pages)",
            "Long" => "detailed (8-10 pages)",
            _ => "standard (4-6 pages)"
        };
        
        var toneText = tone switch
        {
            "Casual" => "casual, conversational",
            "Analytical" => "analytical, data-driven",
            _ => "formal, professional"
        };
        
        var focusText = string.IsNullOrWhiteSpace(focus) ? string.Empty : $"\n\nFocus specifically on: {focus}";
        
        return $"Create a {lengthText} briefing document (Markdown) in {formatText} summarizing the key points, timeline, and stakeholders from the following text. " +
               $"Write it in a {toneText} tone, in the voice of Zara (the ship's AI and tactical officer). " +
               $"Use clear headings, bullet points, and structured sections.{focusText}{extra}\n\nContext:\n{context}";
    }
    
    private static string BuildDataTablePrompt(string extra, string context, string? userInstructions)
    {
        var focus = string.Empty;
        
        // Extract data table settings from user instructions
        if (!string.IsNullOrWhiteSpace(userInstructions))
        {
            if (userInstructions.Contains("Focus:", StringComparison.OrdinalIgnoreCase))
            {
                var focusMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Focus:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (focusMatch.Success) focus = focusMatch.Groups[1].Value.Trim();
            }
        }
        
        var focusText = string.IsNullOrWhiteSpace(focus) ? string.Empty : $"\n\nFocus specifically on: {focus}";
        
        return $"Generate a data table JSON from the provided sources.{focusText}\n\n" +
               $"Extract structured data using natural language processing to identify entities, attributes, and relationships. " +
               $"Use entity extraction to find key data points and organize them into a table format.\n\n" +
               $"Output as JSON object with this exact structure: {{'columns': ['Column1', 'Column2', ...], 'rows': [['Value1', 'Value2', ...], ['Value2', 'Value2', ...]]}}\n\n" +
               $"The columns should be meaningful headers based on the data extracted. The rows should contain the actual data values. " +
               $"Ensure each row has the same number of values as there are columns.\n\n" +
               $"Return ONLY the JSON object (no markdown, no explanations).{extra}\n\nContext:\n{context}";
    }
    
    private static string BuildInfographicPrompt(string extra, string context, string? userInstructions)
    {
        var orientation = "Portrait";
        var detailLevel = "Standard";
        var description = string.Empty;
        
        // Extract infographic settings from user instructions
        if (!string.IsNullOrWhiteSpace(userInstructions))
        {
            if (userInstructions.Contains("Orientation:", StringComparison.OrdinalIgnoreCase))
            {
                var orientMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Orientation:\s*([^\.]+?)(?:\n|\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (orientMatch.Success) orientation = orientMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Detail Level:", StringComparison.OrdinalIgnoreCase))
            {
                var detailMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Detail Level:\s*([^\.]+?)(?:\n|\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (detailMatch.Success) detailLevel = detailMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Description:", StringComparison.OrdinalIgnoreCase))
            {
                var descMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Description:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (descMatch.Success) description = descMatch.Groups[1].Value.Trim();
            }
        }
        
        var numberOfSlides = detailLevel switch
        {
            "Concise" => 4,
            "Detailed" => 8,
            _ => 6
        };
        
        var visualStyle = orientation switch
        {
            "Landscape" => "wide, cinematic",
            "Square" => "balanced, modern",
            _ => "tall, elegant"
        };
        
        var descriptionText = string.IsNullOrWhiteSpace(description) ? string.Empty : $"\n\nFocus specifically on: {description}";
        
        return $"Generate an infographic slideshow as JSON from the provided sources. Create {numberOfSlides} slides with {visualStyle} visual style.{descriptionText}\n\n" +
               $"Format as JSON object: {{'narration': 'A cohesive narrative that ties all slides together, explaining the infographic content in a clear and engaging way.', 'slides': [{{'title': '...', 'content': '...', 'visual_prompt': 'A high-quality, professional infographic image prompt describing the visual design for this slide. Include specific design elements, color schemes, icons, charts, or graphics that would make this slide visually compelling and informative.'}}]}}\n\n" +
               $"Each slide should present a key point or section of the infographic. The visual_prompt should be detailed and describe a professional infographic design with clear visual hierarchy, appropriate icons, charts, or graphics. " +
               $"The narration should be a smooth, engaging voice-over script that explains the infographic content as the slides progress.\n\n" +
               $"Return ONLY the JSON object (no markdown, no explanations).{extra}\n\nContext:\n{context}";
    }
    
    private static string BuildQuizPrompt(string extra, string context, string? userInstructions)
    {
        var numQuestions = 5;
        var difficulty = "Medium";
        var quizType = "Multiple Choice";
        var focus = string.Empty;
        
        // Extract quiz settings from user instructions
        if (!string.IsNullOrWhiteSpace(userInstructions))
        {
            // Handle "Number of Questions: Fewer/Standard/More" format
            if (userInstructions.Contains("Number of Questions:", StringComparison.OrdinalIgnoreCase) || userInstructions.Contains("Number:", StringComparison.OrdinalIgnoreCase))
            {
                var numMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Number(?:\s+of\s+Questions)?:\s*([^\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (numMatch.Success)
                {
                    var numStr = numMatch.Groups[1].Value.Trim();
                    // Handle text values
                    if (numStr.Equals("Fewer", StringComparison.OrdinalIgnoreCase))
                        numQuestions = 4;
                    else if (numStr.Equals("Standard", StringComparison.OrdinalIgnoreCase))
                        numQuestions = 6;
                    else if (numStr.Equals("More", StringComparison.OrdinalIgnoreCase))
                        numQuestions = 9;
                    else if (int.TryParse(numStr, out var num))
                        numQuestions = Math.Clamp(num, 1, 10);
                }
            }
            if (userInstructions.Contains("Difficulty:", StringComparison.OrdinalIgnoreCase))
            {
                var diffMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Difficulty:\s*([^\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (diffMatch.Success) difficulty = diffMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Type:", StringComparison.OrdinalIgnoreCase))
            {
                var typeMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Type:\s*([^\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (typeMatch.Success) quizType = typeMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Description:", StringComparison.OrdinalIgnoreCase))
            {
                var descMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Description:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (descMatch.Success) focus = descMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Focus:", StringComparison.OrdinalIgnoreCase))
            {
                var focusMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Focus:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (focusMatch.Success) focus = focusMatch.Groups[1].Value.Trim();
            }
        }
        
        var difficultyDesc = difficulty switch
        {
            "Easy" => "Use simple, straightforward questions with clear answers.",
            "Hard" => "Use challenging questions that require deep understanding and analysis.",
            _ => "Use moderate difficulty questions that test comprehension."
        };
        
        var typeFormat = quizType switch
        {
            "True/False" => "{'question': '...', 'type': 'True/False', 'answer': 'True' or 'False', 'explanation': '...'}",
            "Short Answer" => "{'question': '...', 'type': 'Short Answer', 'answer': '...', 'explanation': '...'}",
            _ => "{'question': '...', 'type': 'Multiple Choice', 'options': ['option1', 'option2', 'option3', 'option4'], 'answer': 'option1', 'explanation': '...'}"
        };
        
        var focusText = string.IsNullOrWhiteSpace(focus) ? string.Empty : $"\n\nFocus specifically on: {focus}";
        
        return $"Generate a quiz JSON array from the provided sources. Number of questions: {numQuestions}. Difficulty: {difficulty}. {difficultyDesc} Quiz type: {quizType}.{focusText}\n\n" +
               $"Format each question as: {typeFormat}\n\n" +
               $"Return ONLY a JSON array with {numQuestions} questions. Each question must include: question (string), type (string), answer (string), explanation (string), and options (array) if type is Multiple Choice.{extra}\n\nContext:\n{context}";
    }
    
    private static string BuildAudioOverviewPrompt(string extra, string context, string? userInstructions)
    {
        var format = "Deep Dive";
        var length = "Default";
        var focus = string.Empty;
        
        // Extract format, length, and focus from user instructions
        if (!string.IsNullOrWhiteSpace(userInstructions))
        {
            if (userInstructions.Contains("Format:", StringComparison.OrdinalIgnoreCase))
            {
                var formatMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Format:\s*([^\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (formatMatch.Success) format = formatMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Length:", StringComparison.OrdinalIgnoreCase))
            {
                var lengthMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Length:\s*([^\.]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (lengthMatch.Success) length = lengthMatch.Groups[1].Value.Trim();
            }
            if (userInstructions.Contains("Focus:", StringComparison.OrdinalIgnoreCase))
            {
                var focusMatch = System.Text.RegularExpressions.Regex.Match(userInstructions, @"Focus:\s*(.+?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
                if (focusMatch.Success) focus = focusMatch.Groups[1].Value.Trim();
            }
        }
        
        var formatDesc = format switch
        {
            "Brief" => "A bite-sized overview to grasp core ideas quickly—short and to the point, like a status report.",
            "Critique" => "An expert review offering constructive feedback, pointing out strengths and gaps in your material.",
            "Debate" => "A thoughtful debate illuminating different perspectives, like Mal and Simon arguing ethics.",
            _ => "A lively conversation unpacking and connecting topics in your sources, like the crew dissecting a plan."
        };
        
        var lengthDesc = length switch
        {
            "Short" => "Keep it concise, 3-5 minutes of content.",
            "Long" => "Go deeper, 8-12 minutes of content.",
            _ => "Balanced length, 5-8 minutes of content."
        };
        
        var focusText = string.IsNullOrWhiteSpace(focus) ? string.Empty : $"\n\nFocus specifically on: {focus}";
        
        return $"Generate a conversational podcast script between two hosts: Inara (insightful, analytical) and Elias (enthusiastic, curious). They are discussing the provided sources. {formatDesc} {lengthDesc}{focusText}\n\nFormat as a JSON array: [{{'speaker':'Inara','text':'...'}}, {{'speaker':'Elias','text':'...'}}]. The dialogue should feel like a professional podcast, with natural transitions and deep analysis of the source material. Return ONLY JSON.{extra}\n\nContext:\n{context}";
    }
    
    private static string GetPromptForType(ArtifactType type, string context, string? userInstructions)
    {
        var extra = string.IsNullOrWhiteSpace(userInstructions)
            ? string.Empty
            : $"\n\nUser instructions (highest priority):\n{userInstructions.Trim()}";

        return type switch
        {
            ArtifactType.MindMap => BuildMindMapPrompt(extra, context, userInstructions),
            ArtifactType.Quiz => BuildQuizPrompt(extra, context, userInstructions),
            ArtifactType.Flashcards => BuildFlashCardsPrompt(extra, context, userInstructions),
            ArtifactType.SlideDeck => BuildSlideDeckPrompt(extra, context, userInstructions),
            ArtifactType.BriefingDoc => BuildBriefingDocPrompt(extra, context, userInstructions),
            ArtifactType.AudioOverview => BuildAudioOverviewPrompt(extra, context, userInstructions),
            ArtifactType.DataTable => BuildDataTablePrompt(extra, context, userInstructions),
            ArtifactType.Infographic => BuildInfographicPrompt(extra, context, userInstructions),
            ArtifactType.VideoOverview => $"Create a Video Overview plan as JSON: {{\"narration\":\"...\",\"slides\":[{{\"title\":\"...\",\"content\":\"...\",\"visual_prompt\":\"A high-quality image prompt for the background of this slide, based on its content. Include visual style details if specified in user instructions.\"}}]}}. The narration should be a cohesive narrative summary of the sources. Keep it grounded. Return ONLY JSON.{extra}\n\nContext:\n{context}",
            _ => $"Analyze the following text:\n{context}"
        };
    }
}

