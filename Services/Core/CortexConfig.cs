using System;
using System.Collections.Generic;
using System.IO;

namespace Serenity.Cortex.Core.Config;

/// <summary>
/// Lightweight config reader for Cortex Core.
/// Reads from environment variables first, then from serenity.env (repo or LocalAppData).
/// </summary>
public static class CortexConfig
{
    private static readonly object Gate = new();
    private static bool _loaded;
    private static Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    public static string? LoadedPath { get; private set; }

    public static string? Get(string key, string? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(key)) return defaultValue;

        // Env wins.
        var env = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(env)) return env.Trim();

        EnsureLoaded();
        return _values.TryGetValue(key.Trim(), out var v) ? v : defaultValue;
    }

    public static bool GetBoolean(string key, bool defaultValue = false)
    {
        var v = Get(key);
        if (string.IsNullOrWhiteSpace(v)) return defaultValue;
        return bool.TryParse(v.Trim(), out var b) ? b : defaultValue;
    }

    public static void Reload()
    {
        lock (Gate)
        {
            _loaded = false;
            _values = new(StringComparer.OrdinalIgnoreCase);
            LoadedPath = null;
        }
    }

    private static void EnsureLoaded()
    {
        if (_loaded) return;

        lock (Gate)
        {
            if (_loaded) return;

            var candidatePaths = new List<string>();

            // 1) LocalAppData fallback (always writable).
            try
            {
                var localRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrWhiteSpace(localRoot))
                {
                    candidatePaths.Add(Path.Combine(localRoot, "Serenity", "serenity.env"));
                }
            }
            catch { }

            // 2) App-local config/serenity.env.
            try
            {
                var baseDir = AppContext.BaseDirectory;
                if (!string.IsNullOrWhiteSpace(baseDir))
                {
                    candidatePaths.Add(Path.Combine(baseDir, "config", "serenity.env"));
                }
            }
            catch { }

            // 3) Walk up from BaseDirectory looking for config/serenity.env (dev builds).
            try
            {
                var found = FindUpwards(AppContext.BaseDirectory, Path.Combine("config", "serenity.env"), maxDepth: 8);
                if (!string.IsNullOrWhiteSpace(found)) candidatePaths.Add(found);
            }
            catch { }

            foreach (var path in candidatePaths)
            {
                if (TryLoad(path))
                {
                    LoadedPath = path;
                    _loaded = true;
                    return;
                }
            }

            // Nothing found: keep empty config.
            _loaded = true;
        }
    }

    private static string? FindUpwards(string? startDir, string relativeTarget, int maxDepth)
    {
        if (string.IsNullOrWhiteSpace(startDir)) return null;
        var current = startDir;
        for (int i = 0; i < Math.Max(1, maxDepth); i++)
        {
            var candidate = Path.GetFullPath(Path.Combine(current, relativeTarget));
            if (File.Exists(candidate)) return candidate;

            var parent = Directory.GetParent(current);
            if (parent == null) break;
            current = parent.FullName;
        }
        return null;
    }

    private static bool TryLoad(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return false;

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var line = raw.Trim();
                if (line.StartsWith("#")) continue;

                var parts = line.Split('=', 2);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();
                if (key.Length == 0) continue;
                dict[key] = value;
            }

            _values = dict;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
