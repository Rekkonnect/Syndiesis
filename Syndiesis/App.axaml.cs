using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.CodeAnalysis;
using Serilog;
using Serilog.Events;
using Syndiesis.Utilities;
using Syndiesis.Views;
using System;
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
        AppSettings.TryLoad();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            SetupDesktop(desktop);
        }

        if (ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            SetupSingleView(single);
        }

        if (ApplicationLifetime is IControlledApplicationLifetime controlled)
        {
            SetupControlled(controlled);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void SetupDesktop(IClassicDesktopStyleApplicationLifetime desktop)
    {
        desktop.MainWindow = new MainWindow();
    }

    private void SetupSingleView(ISingleViewApplicationLifetime single)
    {
        single.MainView = new MainView();
    }

    private void SetupControlled(IControlledApplicationLifetime controlled)
    {
        SetupSerilog(controlled);
        controlled.Exit += HandleControlledLifetimeExit;
    }

    private void HandleControlledLifetimeExit(
        object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        AppSettings.TrySave();
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
                outputTemplate: LoggerExtensionsEx.DefaultOutputTemplate)
            .CreateLogger()
            ;

        Log.Information("---- Application is starting -- Serilog was setup");
    }

    private static void LogApplicationExit(object? sender, EventArgs e)
    {
        LoggerExtensionsEx.LogMethodInvocation(nameof(LogApplicationExit));
    }
}
