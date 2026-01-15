using System.Diagnostics;

namespace Cortex.App.Helpers;

public static class FileHelper
{
    public static void ShellOpen(string path)
    {
        try
        {
            if (System.IO.Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            else if (System.IO.File.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = System.IO.Path.GetDirectoryName(path) ?? path,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // Silently fail if shell open is not supported
        }
    }
}
