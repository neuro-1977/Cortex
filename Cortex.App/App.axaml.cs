using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Serenity.Diagnostics;

namespace Serenity;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            Diagnostics.AgentLogService.WriteLaunch("Framework initialization starting...");
            
            Dispatcher.UIThread.UnhandledException += (_, e) =>
            {
                CrashLog.Write("Dispatcher.UIThread.UnhandledException", e.Exception);
                Diagnostics.AgentLogService.WriteLaunch("UI Thread unhandled exception", e.Exception);
                Diagnostics.AgentLogService.WriteDebugger("UI Thread exception", e.Exception);
                // Let Avalonia continue its default behavior; we're primarily collecting diagnostics.
            };

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                Diagnostics.AgentLogService.WriteLaunch("Creating MainViewModel...");
                ViewModels.MainViewModel? vm = null;
                try
                {
                    vm = new ViewModels.MainViewModel();
                    Diagnostics.AgentLogService.WriteLaunch("MainViewModel created successfully");
                }
                catch (Exception ex)
                {
                    CrashLog.Write("MainViewModel.ctor", ex);
                    Diagnostics.AgentLogService.WriteLaunch("CRITICAL: MainViewModel creation failed", ex);
                    Diagnostics.AgentLogService.WriteDebugger("MainViewModel constructor error", ex);
                    throw;
                }
                
                Diagnostics.AgentLogService.WriteLaunch("Creating MainWindow...");
                try
                {
                    desktop.MainWindow = new Views.MainWindow
                    {
                        DataContext = vm
                    };
                    Diagnostics.AgentLogService.WriteLaunch("MainWindow created successfully");
                }
                catch (Exception ex)
                {
                    CrashLog.Write("MainWindow.ctor", ex);
                    Diagnostics.AgentLogService.WriteLaunch("CRITICAL: MainWindow creation failed", ex);
                    Diagnostics.AgentLogService.WriteDebugger("MainWindow constructor error", ex);
                    throw;
                }
                
                // Start Discord bot on app startup
                desktop.Startup += async (_, _) =>
                {
                    try
                    {
                        Diagnostics.AgentLogService.WriteLaunch("Application startup event fired");
                        await vm.StartDiscordBotCommand.ExecuteAsync(null);
                        Diagnostics.AgentLogService.WriteLaunch("Startup completed successfully");
                    }
                    catch (Exception ex)
                    {
                        CrashLog.Write("App.Startup", ex);
                        Diagnostics.AgentLogService.WriteLaunch("ERROR: Startup event failed", ex);
                        Diagnostics.AgentLogService.WriteDebugger("Startup event error", ex);
                    }
                };
                
                // Stop Discord bot and cleanup on app shutdown
                desktop.Exit += (_, _) =>
                {
                    try
                    {
                        Diagnostics.AgentLogService.WriteLaunch("Application shutdown event fired");
                        vm.StopDiscordBotCommand.Execute(null);
                        vm.CleanupFileWatchers();
                        Diagnostics.AgentLogService.WriteLaunch("Shutdown completed");
                    }
                    catch (Exception ex)
                    {
                        CrashLog.Write("App.Exit", ex);
                        Diagnostics.AgentLogService.WriteLaunch("ERROR: Exit event failed", ex);
                    }
                };
                
                Diagnostics.AgentLogService.WriteLaunch("Framework initialization completed successfully");
            }
            else
            {
                Diagnostics.AgentLogService.WriteLaunch("WARNING: ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime");
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            CrashLog.Write("App.OnFrameworkInitializationCompleted", ex);
            Diagnostics.AgentLogService.WriteLaunch("CRITICAL: Framework initialization failed", ex);
            Diagnostics.AgentLogService.WriteDebugger("Framework initialization error", ex);
            Diagnostics.AgentLogService.WriteErrorChecker("Framework initialization error", ex);
            throw;
        }
    }
}
