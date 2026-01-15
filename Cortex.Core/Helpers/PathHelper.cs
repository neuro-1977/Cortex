using System;
using System.IO;

namespace Cortex.Core.Helpers;

public static class PathHelper
{
    public static string GetOutputDir(string subfolder)
    {
        var baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Cortex",
            "output",
            subfolder
        );
        Directory.CreateDirectory(baseDir);
        return baseDir;
    }

    public static string GetProjectRoot()
    {
        // Try to find the project root by looking for config/cortex.env
        var current = AppContext.BaseDirectory;
        for (int i = 0; i < 8; i++)
        {
            var configPath = Path.Combine(current, "config", "cortex.env");
            if (File.Exists(configPath))
            {
                return current;
            }
            var parent = Directory.GetParent(current);
            if (parent == null) break;
            current = parent.FullName;
        }
        // Fallback to BaseDirectory
        return AppContext.BaseDirectory;
    }
}
