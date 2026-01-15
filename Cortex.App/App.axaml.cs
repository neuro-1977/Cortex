using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using Cortex.App.ViewModels;

namespace Cortex.App;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            Dispatcher.UIThread.UnhandledException += (_, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"UI Thread unhandled exception: {e.Exception}");
                // Let Avalonia continue its default behavior
            };

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                try
                {
                    var vm = new CortexViewModel();
                    desktop.MainWindow = new Views.MainWindow
                    {
                        DataContext = vm
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CRITICAL: MainWindow creation failed: {ex}");
                    throw;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CRITICAL: Framework initialization failed: {ex}");
            throw;
        }
    }
}
