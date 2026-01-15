using Avalonia;
using System;
using LibVLCSharp.Shared;

namespace Cortex.App;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            // Initialize LibVLC for video avatars
            LibVLCSharp.Shared.Core.Initialize();
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args ?? Array.Empty<string>());
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Cortex startup error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.ExitCode = 1;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
