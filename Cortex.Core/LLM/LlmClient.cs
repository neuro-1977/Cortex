using Cortex.Core.Config;
using Cortex.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cortex.Core.LLM;

public sealed class LlmClient : ILlmClient
{
    // Enhanced timeout handling - configurable via CortexConfig, with sensible defaults
    private static readonly HttpClient Http = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(10),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        ConnectTimeout = TimeSpan.FromSeconds(10) // Fast connect timeout
    })
    {
        Timeout = TimeSpan.FromSeconds(int.TryParse(Config.CortexConfig.Get("LLM_TIMEOUT_SECONDS", "90"), out var timeout) ? timeout : 90)
    };

    private static readonly SemaphoreSlim Concurrency = new(2, 2);

    public async Task<string> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) return "Error: request is null.";
        var prompt = SanitizePrompt(request.Prompt);
        if (string.IsNullOrWhiteSpace(prompt)) return "Error: empty prompt.";

        var model = (request.Model ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(model)) model = ResolveDefaultModel();
        
        var systemPrompt = request.SystemPrompt;

        await Concurrency.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (LooksLikeOllama(model))
            {
                return await GenerateWithOllamaAsync(prompt, model, systemPrompt, cancellationToken).ConfigureAwait(false);
            }

            if (LooksLikeGrok(model))
            {
                return await GenerateWithGrokAsync(prompt, model, request.Temperature, systemPrompt, cancellationToken).ConfigureAwait(false);
            }

            // Default: Gemini.
            return await GenerateWithGeminiAsync(prompt, model, request.Temperature, systemPrompt, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return "Error: request canceled.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
        finally
        {
            Concurrency.Release();
        }
    }

    private static string ResolveDefaultModel()
    {
        var configured = CortexConfig.Get("CORTEX_MODEL");
        if (!string.IsNullOrWhiteSpace(configured)) return configured.Trim();

        if (!string.IsNullOrWhiteSpace(CortexConfig.Get("GEMINI_API_KEY"))) return "gemini-2.0-flash-exp";
        if (!string.IsNullOrWhiteSpace(CortexConfig.Get("XAI_API_KEY")) || !string.IsNullOrWhiteSpace(CortexConfig.Get("GROK_API_KEY"))) return "grok-2-latest";

        return "ollama:phi3";
    }

    private static bool LooksLikeOllama(string model) => model.StartsWith("ollama:", StringComparison.OrdinalIgnoreCase);

    private static bool LooksLikeGrok(string model)
    {
        var m = model.Trim().ToLowerInvariant();
        return m.StartsWith("grok") || m.Contains("xai") || m.Contains("x.ai");
    }

    private static string SanitizePrompt(string? prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt)) return string.Empty;
        var p = prompt.Trim();
        const int maxChars = 128_000;
        if (p.Length > maxChars) p = p.Substring(0, maxChars);
        return p;
    }

    private static async Task<string> GenerateWithOllamaAsync(string prompt, string model, string? systemPrompt, CancellationToken ct)
    {
        // model is "ollama:<name>"
        var name = model.Substring("ollama:".Length).Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "phi3";

        var host = CortexConfig.Get("OLLAMA_HOST", "http://localhost:11434")!.Trim().TrimEnd('/');
        var url = host + "/api/generate";

        var payload = new
        {
            model = name,
            prompt,
            system = systemPrompt,
            stream = false
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await NetworkRetryHelper.ExecuteWithRetryAsync(
            async (cancellationToken) => await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false),
            maxRetries: 3,
            initialDelayMs: 1000,
            maxDelayMs: 10000,
            cancellationToken: ct).ConfigureAwait(false);
        
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
        {
            return $"Error: Ollama returned {(int)resp.StatusCode}.";
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var r) && r.ValueKind == JsonValueKind.String)
            {
                return r.GetString() ?? string.Empty;
            }
        }
        catch { }

        // Some Ollama versions return JSONL even with stream=false; try to stitch lines.
        var stitched = TryStitchOllamaJsonl(body);
        if (!string.IsNullOrWhiteSpace(stitched)) return stitched;

        return body;
    }

    private static string? TryStitchOllamaJsonl(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        var sb = new StringBuilder();
        var lines = body.Replace("\r\n", "\n").Split('\n');
        foreach (var line in lines)
        {
            var t = line.Trim();
            if (t.Length == 0) continue;
            try
            {
                using var doc = JsonDocument.Parse(t);
                if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("response", out var r) && r.ValueKind == JsonValueKind.String)
                {
                    sb.Append(r.GetString());
                }
            }
            catch { }
        }
        var s = sb.ToString();
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    private static async Task<string> GenerateWithGeminiAsync(string prompt, string model, double temperature, string? systemPrompt, CancellationToken ct)
    {
        var apiKey = CortexConfig.Get("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) return "Error: Gemini is not configured (missing GEMINI_API_KEY).";

        // Allow shorthand "gemini".
        var m = model.Trim();
        if (m.Equals("gemini", StringComparison.OrdinalIgnoreCase)) m = "gemini-2.0-flash-exp";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(m)}:generateContent?key={Uri.EscapeDataString(apiKey)}";

        // Construct payload with optional system_instruction
        object? systemInstruction = null;
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            };
        }

        var payload = new
        {
            system_instruction = systemInstruction,
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = Math.Clamp(temperature, 0.0, 2.0)
            }
        };

        // If system_instruction is null, the serializer will omit it if configured, or include null. 
        // Gemini API might reject null system_instruction.
        // It's cleaner to use a Dictionary or similar, but let's just use conditional serialization or simpler object.
        // We'll trust the JsonSerializer to handle it or we can be explicit. 
        // If it fails, we can fallback to prepending text.
        
        var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
        
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Content = new StringContent(JsonSerializer.Serialize(payload, jsonOptions), Encoding.UTF8, "application/json");

        using var resp = await NetworkRetryHelper.ExecuteWithRetryAsync(
            async (cancellationToken) => await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false),
            maxRetries: 3,
            initialDelayMs: 1000,
            maxDelayMs: 30000,
            cancellationToken: ct).ConfigureAwait(false);
        
        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
        {
            return $"Error: Gemini returned {(int)resp.StatusCode}.";
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var candidates = doc.RootElement.TryGetProperty("candidates", out var c) ? c : default;
            if (candidates.ValueKind == JsonValueKind.Array && candidates.GetArrayLength() > 0)
            {
                var content = candidates[0].TryGetProperty("content", out var cc) ? cc : default;
                if (content.ValueKind == JsonValueKind.Object && content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                {
                    var sb = new StringBuilder();
                    foreach (var p in parts.EnumerateArray())
                    {
                        if (p.ValueKind == JsonValueKind.Object && p.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                        {
                            sb.Append(t.GetString());
                        }
                    }
                    return sb.ToString();
                }
            }
        }
        catch { }

        return body;
    }

    private static async Task<string> GenerateWithGrokAsync(string prompt, string model, double temperature, string? systemPrompt, CancellationToken ct)
    {
        var apiKey = CortexConfig.Get("XAI_API_KEY") ?? CortexConfig.Get("GROK_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) return "Error: Grok is not configured (missing XAI_API_KEY).";

        var endpoint = CortexConfig.Get("XAI_ENDPOINT", "https://api.x.ai/v1/chat/completions")!.Trim();

        // Allow shorthand "grok".
        var m = model.Trim();
        if (m.Equals("grok", StringComparison.OrdinalIgnoreCase))
        {
            m = CortexConfig.Get("XAI_MODEL", "grok-2-latest")!.Trim();
        }

        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(new { role = "system", content = systemPrompt });
        }
        messages.Add(new { role = "user", content = prompt });

        var payload = new
        {
            model = m,
            messages = messages,
            temperature = Math.Clamp(temperature, 0.0, 2.0)
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);

        var ctHeader = resp.Content.Headers.ContentType?.MediaType;
        if (ctHeader != null && !ctHeader.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            return "Error: Grok returned a non-JSON response.";
        }

        var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
        {
            return $"Error: Grok returned {(int)resp.StatusCode}.";
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var msg = choices[0].TryGetProperty("message", out var mEl) ? mEl : default;
                if (msg.ValueKind == JsonValueKind.Object && msg.TryGetProperty("content", out var cEl) && cEl.ValueKind == JsonValueKind.String)
                {
                    return cEl.GetString() ?? string.Empty;
                }
            }
        }
        catch { }

        return body;
    }
}

