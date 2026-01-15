using System;
using System.IO;

namespace Cortex.Core.Services;

public static class CortexFileCleanupService
{
    public static bool TryDeleteGeneratedPath(string? path, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(path)) return false;

        try
        {
            var full = Path.GetFullPath(path);
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Serenity", "Cortex");
            var baseFull = Path.GetFullPath(baseDir) + Path.DirectorySeparatorChar;

            // Only delete files under %LocalAppData%\Serenity\Cortex\ to avoid destructive surprises.
            if (!full.StartsWith(baseFull, StringComparison.OrdinalIgnoreCase))
            {
                error = "Refusing to delete file outside Serenity\\Cortex output directory.";
                return false;
            }

            if (!File.Exists(full)) return false;
            File.Delete(full);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static void TryDeleteGeneratedPaths(string? filePath, string? visualPath)
    {
        _ = TryDeleteGeneratedPath(filePath, out _);
        _ = TryDeleteGeneratedPath(visualPath, out _);
    }
}

