using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Garyon.Objects;
using Microsoft.CodeAnalysis;
using Serilog;
using Serilog.Events;
using Syndiesis.Updating;
using Syndiesis.Utilities;
using Syndiesis.Views;
using System.Reflection;

namespace Syndiesis;

public partial class App : Application
{
    public const LogEventLevel DefaultLogEventLevel =
#if DEBUG
        LogEventLevel.Debug
#else
        LogEventLevel.Information
#endif
        ;

    public static new App Current => (Application.Current as App)!;

    public static AppResourceManager CurrentResourceManager
        => Current.ResourceManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public AppResourceManager ResourceManager { get; private set; }

    public AppInfo AppInfo { get; private set; }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public ExceptionListener ExceptionListener { get; } = new();

    public override void Initialize()
    {
        // Force initialize app settings on the UI thread to avoid troubles
        // with extra invocation handling when initializing instances from
        // de-/serializations or first-time accesses. Deadlocks are very
        // common to encounter when incorrectly using UI thread dispatches
        _ = AppSettings.Instance;
        AvaloniaXamlLoader.Load(this);
        ResourceManager = new(this);
        AppInfo = CreateAppInfo();
        SetupGeneral();
    }

    private AppInfo CreateAppInfo()
    {
        var assembly = GetType().Assembly;
        var roslynAssembly = typeof(SyntaxNode).Assembly;
        return new()
        {
            InformationalVersion = InformationalVersionForAssembly(assembly),
            RoslynVersion = InformationalVersionForAssembly(roslynAssembly),
        };
    }

    private static InformationalVersion InformationalVersionForAssembly(Assembly assembly)
    {
        return InformationalVersion.Parse(
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!);
    }

    private static void SetupGeneral()
    {
        SetupSerilog();
        Task.Run(SetupGeneralAsync);
    }

    private static async Task SetupGeneralAsync()
    {
        await AppSettings.TryLoad();
        await CheckUpdates();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            SetupDesktopLifetime(desktop);
        }

        if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            SetupSingleViewLifetime(single);
        }

        if (ApplicationLifetime is IControlledApplicationLifetime controlled)
        {
            SetupControlledLifetime(controlled);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupDesktopLifetime(IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = new MainWindow();
    }

    private void SetupSingleViewLifetime(ISingleViewApplicationLifetime single)
    {
        single.MainView = new MainView();
    }

    private void SetupControlledLifetime(IControlledApplicationLifetime controlled)
    {
        SetupSerilog(controlled);
        controlled.Exit += HandleControlledLifetimeExit;
    }

    private void HandleControlledLifetimeExit(
        object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Task.Run(() => AppSettings.TrySave());
    }

    private void SetupSerilog(IControlledApplicationLifetime lifetime)
    {
        lifetime.Exit += LogApplicationExit;
    }

    private static void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(DefaultLogEventLevel)
            .WriteTo.File(
                "logs/syndiesis-main.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: LoggerExtensionsEx.DefaultOutputTemplate,
                buffered: true,
                flushToDiskInterval: TimeSpan.FromSeconds(2))
            .CreateLogger()
            ;

        Log.Information("---- Application is starting -- Serilog was setup");
    }

    private static void LogApplicationExit(object? sender, EventArgs e)
    {
        LoggerExtensionsEx.LogMethodInvocation(nameof(LogApplicationExit));
    }

    private static async Task CheckUpdates()
    {
        if (AppSettings.Instance.UpdateOptions.AutoCheckUpdates)
        {
            await Singleton<UpdateManager>.Instance.CheckForUpdates();
        }
    }
}
