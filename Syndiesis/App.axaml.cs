using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Syndiesis.ViewModels;
using Syndiesis.Views;

namespace Syndiesis;

public partial class App : Application
{
    public static AppResourceManager CurrentResourceManager
        => (Current as App)!.ResourceManager;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public AppResourceManager ResourceManager { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ResourceManager = new(this);
        AppSettings.TryLoad();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
