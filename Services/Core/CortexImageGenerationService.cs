using Serenity.Cortex.Core.Config;
using Serenity.Cortex.Core.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Serenity.Cortex.Core.Services;

public sealed class CortexImageGenerationService
{
    // Enhanced timeout handling - configurable via CortexConfig
    private static readonly HttpClient Http = new(new SocketsHttpHandler
    {
        ConnectTimeout = TimeSpan.FromSeconds(10),
        PooledConnectionLifetime = TimeSpan.FromMinutes(5)
    })
    {
        Timeout = TimeSpan.FromSeconds(int.TryParse(Config.CortexConfig.Get("IMAGE_GEN_TIMEOUT_SECONDS", "120"), out var timeout) ? timeout : 120)
    };

    private static string GetBaseDir()
    {
        return PathHelper.GetOutputDir("images");
    }

    public bool IsConfigured()
    {
        // Check for Stable Diffusion (local, preferred) or Grok/xAI (fallback)
        var stableDiffusionUrl = CortexConfig.Get("STABLE_DIFFUSION_URL");
        if (!string.IsNullOrWhiteSpace(stableDiffusionUrl)) return true;
        
        // Fallback to Grok/xAI if no Stable Diffusion
        var xaiKey = CortexConfig.Get("XAI_API_KEY") ?? CortexConfig.Get("GROK_API_KEY");
        return !string.IsNullOrWhiteSpace(xaiKey);
    }
    
    private bool UseStableDiffusion()
    {
        var stableDiffusionUrl = CortexConfig.Get("STABLE_DIFFUSION_URL");
        return !string.IsNullOrWhiteSpace(stableDiffusionUrl);
    }

    public async Task<string?> GenerateImageAsync(string artifactId, string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            Debug.WriteLine("[CortexImageGenerationService] GenerateImageAsync: Prompt is null or empty");
            return null;
        }

        var baseDir = GetBaseDir();
        if (string.IsNullOrWhiteSpace(artifactId))
        {
            Debug.WriteLine("[CortexImageGenerationService] GenerateImageAsync: ArtifactId is null or empty, generating new GUID");
            artifactId = Guid.NewGuid().ToString();
        }
        var outPath = Path.Combine(baseDir, $"{artifactId}.png");

        // Prefer Stable Diffusion if configured (local, free, no API costs)
        if (UseStableDiffusion())
        {
            return await GenerateWithStableDiffusionAsync(prompt, outPath, cancellationToken);
        }

        // Fallback to Grok/xAI only if Stable Diffusion not available
        var apiKey = CortexConfig.Get("XAI_API_KEY") ?? CortexConfig.Get("GROK_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        return await GenerateWithGrokAsync(prompt, outPath, apiKey, cancellationToken);
    }
    
    private async Task<string?> GenerateWithStableDiffusionAsync(string prompt, string outPath, CancellationToken cancellationToken)
    {
        // Default to local Stable Diffusion WebUI
        var stableDiffusionUrl = CortexConfig.Get("STABLE_DIFFUSION_URL") ?? "http://127.0.0.1:7860";
        var endpoint = $"{stableDiffusionUrl.TrimEnd('/')}/sdapi/v1/txt2img";
        
        // Get model and other settings from config with sensible defaults
        var model = CortexConfig.Get("STABLE_DIFFUSION_MODEL");
        var width = int.TryParse(CortexConfig.Get("STABLE_DIFFUSION_WIDTH"), out var w) ? w : 1024;
        var height = int.TryParse(CortexConfig.Get("STABLE_DIFFUSION_HEIGHT"), out var h) ? h : 1024;
        var steps = int.TryParse(CortexConfig.Get("STABLE_DIFFUSION_STEPS"), out var s) ? s : 20;
        var cfgScale = double.TryParse(CortexConfig.Get("STABLE_DIFFUSION_CFG_SCALE"), out var cfg) ? cfg : 7.0;
        
        var payload = new
        {
            prompt = prompt,
            negative_prompt = CortexConfig.Get("STABLE_DIFFUSION_NEGATIVE_PROMPT") ?? "blurry, low quality, distorted, watermark, text",
            width = width,
            height = height,
            steps = steps,
            cfg_scale = cfgScale,
            sampler_name = CortexConfig.Get("STABLE_DIFFUSION_SAMPLER") ?? "DPM++ 2M Karras",
            seed = -1, // Random seed
            batch_size = 1,
            n_iter = 1,
            enable_hr = false,
            denoising_strength = 0.7
        };
        
        // If model is specified, set it first (for Automatic1111)
        if (!string.IsNullOrWhiteSpace(model))
        {
            try
            {
                var optionsEndpoint = $"{stableDiffusionUrl.TrimEnd('/')}/sdapi/v1/options";
                var optionsPayload = new { sd_model_checkpoint = model };
                using var optionsReq = new HttpRequestMessage(HttpMethod.Post, optionsEndpoint);
                optionsReq.Content = new StringContent(JsonSerializer.Serialize(optionsPayload), Encoding.UTF8, "application/json");
                using var optionsResp = await NetworkRetryHelper.ExecuteWithRetryAsync(
                    async (ct) => await Http.SendAsync(optionsReq, ct).ConfigureAwait(false),
                    maxRetries: 2,
                    initialDelayMs: 500,
                    maxDelayMs: 5000,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!optionsResp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[CortexImage] Failed to set Stable Diffusion model: {(int)optionsResp.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CortexImage] Error setting Stable Diffusion model: {ex.Message}");
                // Continue anyway - model might already be set
            }
        }
        
        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        
        using var resp = await NetworkRetryHelper.ExecuteWithRetryAsync(
            async (ct) => await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false),
            maxRetries: 3,
            initialDelayMs: 2000,
            maxDelayMs: 30000,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        
        if (!resp.IsSuccessStatusCode)
        {
            var errorBody = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Debug.WriteLine($"[CortexImage] Stable Diffusion API returned {(int)resp.StatusCode}: {errorBody}");
            return null;
        }
        
        var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var doc = JsonDocument.Parse(body);
            var images = doc.RootElement.TryGetProperty("images", out var imgs) ? imgs : default;
            if (images.ValueKind == JsonValueKind.Array && images.GetArrayLength() > 0)
            {
                var b64 = images[0].GetString();
                if (!string.IsNullOrWhiteSpace(b64))
                {
                    // Remove data URL prefix if present
                    if (b64.StartsWith("data:image"))
                    {
                        var commaIndex = b64.IndexOf(',');
                        if (commaIndex >= 0) b64 = b64.Substring(commaIndex + 1);
                    }
                    
                    var bytes = Convert.FromBase64String(b64);
                    await File.WriteAllBytesAsync(outPath, bytes, cancellationToken).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"[CortexImage] Generated image saved to: {outPath}");
                    return outPath;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CortexImage] Stable Diffusion parse error: {ex.Message}");
            return null;
        }
        
        return null;
    }
    
    private async Task<string?> GenerateWithGrokAsync(string prompt, string outPath, string apiKey, CancellationToken cancellationToken)
    {
        // Using xAI's OpenAI-compatible image generation endpoint
        var endpoint = "https://api.x.ai/v1/images/generations";

        var payload = new
        {
            model = "grok-2-vision-1212", // Default image-capable model for Grok
            prompt = prompt,
            n = 1,
            size = "1024x1024",
            response_format = "b64_json"
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await NetworkRetryHelper.ExecuteWithRetryAsync(
            async (ct) => await Http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false),
            maxRetries: 3,
            initialDelayMs: 2000,
            maxDelayMs: 30000,
            cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var doc = JsonDocument.Parse(body);
            var data = doc.RootElement.TryGetProperty("data", out var d) ? d : default;
            if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0)
            {
                var b64 = data[0].TryGetProperty("b64_json", out var b) ? b.GetString() : null;
                if (!string.IsNullOrWhiteSpace(b64))
                {
                    var bytes = Convert.FromBase64String(b64);
                    await File.WriteAllBytesAsync(outPath, bytes, cancellationToken).ConfigureAwait(false);
                    return outPath;
                }
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
