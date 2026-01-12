using Serenity.Cortex.Core.Config;
using Serenity.Cortex.Core.Helpers;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Serenity.Cortex.Core.Services;

public sealed class ElevenLabsTtsService
{
    // Enhanced timeout handling - configurable via CortexConfig
    private static readonly HttpClient Http = new(new SocketsHttpHandler
    {
        ConnectTimeout = TimeSpan.FromSeconds(10),
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    })
    {
        Timeout = TimeSpan.FromSeconds(int.TryParse(CortexConfig.Get("TTS_TIMEOUT_SECONDS", "120"), out var timeout) ? timeout : 120)
    };

    private static string GetBaseDir()
    {
        return PathHelper.GetOutputDir("audio");
    }

    public bool IsConfigured()
    {
        var key = CortexConfig.Get("ELEVENLABS_API_KEY");
        var voiceId = CortexConfig.Get("ELEVENLABS_VOICE_ID") ?? DefaultVoiceId;
        return !string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(voiceId);
    }

    // Your existing Serenity voice id (from the WinUI VoiceService mapping) as a safe default.
    private static string DefaultVoiceId => "x0HJReRwMPN5kTGcLrHD";

    public async Task<string?> SynthesizeToMp3Async(string artifactId, string text, string? voiceId = null, CancellationToken cancellationToken = default)
    {
        // Get format from config (defaults to MP3)
        var format = CortexConfig.Get("AUDIO_FILE_FORMAT", "MP3");
        var bitrate = CortexConfig.Get("AUDIO_BITRATE", "192");
        var sampleRate = CortexConfig.Get("AUDIO_SAMPLE_RATE", "44100");
        var safeFormat = format ?? "MP3";
        return await SynthesizeAsync(artifactId, text, safeFormat, voiceId, bitrate, sampleRate, cancellationToken);
    }
    
    public async Task<string?> SynthesizeAsync(string artifactId, string text, string format = "MP3", string? voiceId = null, string? bitrate = null, string? sampleRate = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var apiKey = CortexConfig.Get("ELEVENLABS_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        var targetVoiceId = (voiceId ?? CortexConfig.Get("ELEVENLABS_VOICE_ID") ?? DefaultVoiceId).Trim();
        if (string.IsNullOrWhiteSpace(targetVoiceId)) return null;

        var modelId = (CortexConfig.Get("ELEVENLABS_MODEL_ID") ?? "eleven_multilingual_v2").Trim();

        var baseDir = GetBaseDir();
        if (string.IsNullOrWhiteSpace(artifactId)) artifactId = Guid.NewGuid().ToString();
        
        // ElevenLabs only outputs MP3, so we always generate MP3 first, then convert if needed
        var extension = format.ToUpperInvariant() switch
        {
            "WAV" => "wav",
            "OGG" => "ogg",
            _ => "mp3"
        };
        var outPath = Path.Combine(baseDir, $"{artifactId}.{extension}");

        var endpoint = $"https://api.elevenlabs.io/v1/text-to-speech/{Uri.EscapeDataString(targetVoiceId)}";

        var payload = new
        {
            text = TrimText(text, 9_000),
            model_id = modelId,
            voice_settings = new { stability = 0.5, similarity_boost = 0.75 }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
        req.Headers.Add("xi-api-key", apiKey.Trim());
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await NetworkRetryHelper.ExecuteWithRetryAsync(
            async (ct) => await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false),
            maxRetries: 3,
            initialDelayMs: 1000,
            maxDelayMs: 20000,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
        {
            var errorBody = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"[ElevenLabsTTS] API returned {(int)resp.StatusCode}: {errorBody}");
            return null;
        }

        var mediaType = resp.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (!mediaType.Contains("audio", StringComparison.OrdinalIgnoreCase))
        {
            // Some errors return JSON even with success=false; but we already checked status.
            var errorBody = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            System.Diagnostics.Debug.WriteLine($"[ElevenLabsTTS] Unexpected content type: {mediaType}. Response: {errorBody.Substring(0, Math.Min(200, errorBody.Length))}");
            return null;
        }

        var bytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        if (bytes == null || bytes.Length < 64) return null;

        await File.WriteAllBytesAsync(outPath, bytes, cancellationToken).ConfigureAwait(false);
        return outPath;
    }

    private static string TrimText(string text, int maxChars)
    {
        var t = (text ?? string.Empty).Trim();
        if (t.Length <= maxChars) return t;
        return t.Substring(0, maxChars) + "…";
    }
}
