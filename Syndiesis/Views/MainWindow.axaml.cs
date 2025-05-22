using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Threading;
using Garyon.Objects;
using Microsoft.CodeAnalysis;
using Syndiesis.Core;
using Syndiesis.Updating;
using System.Threading.Tasks;

namespace Syndiesis.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // Truly a shame
#if DEBUG
        InitializeComponent(attachDevTools: false);
#else
        InitializeComponent();
#endif
        AttachDevTools();
        InitializeEvents();
        SetCurrentTitle();

        InitializeUpdateCheck();
    }

    private void InitializeUpdateCheck()
    {
        Task.Run(Singleton<UpdateManager>.Instance.CheckForUpdates);
    }

    private void AttachDevTools()
    {
#if DEBUG
        var options = new DevToolsOptions
        {
            Gesture = new(Key.F10),
        };
        this.AttachDevTools(options);
#endif
    }

    private void SetCurrentTitle()
    {
        var currentLanguage = GetCurrentLanguageName();
        SetTitleForLanguage(currentLanguage);
    }

    private static string ProgramTitleForLanguage(string languageName)
    {
        return languageName switch
        {
            LanguageNames.CSharp => "Syndiesis",
            LanguageNames.VisualBasic => "SymVBiosis",
            _ => throw RoslynExceptions
                .ThrowInvalidLanguageArgument(languageName, nameof(languageName)),
        };
    }

    private void SetTitleForLanguage(string languageName)
    {
        var title = ProgramTitleForLanguage(languageName);
        SetTitle(title);
    }

    private void SetTitle(string programTitle)
    {
        var infoVersion = App.Current.AppInfo.InformationalVersion;
        Title = $"{programTitle} v{infoVersion.Version} [{infoVersion.CommitSha![..7]}]";
    }

    private void InitializeEvents()
    {
        mainView.MainView.ViewModel.HybridCompilationSource
            .CompilationSourceChanged += OnCompilationSourceChanged;
    }

    private string GetCurrentLanguageName()
    {
        return mainView.MainView.ViewModel.CurrentLanguage;
    }

    private void OnCompilationSourceChanged()
    {
        Dispatcher.UIThread.InvokeAsync(UpdateLogo);
    }

    private void UpdateLogo()
    {
        var image = mainView?.TitleBar?.LogoImage;
        if (image is not null)
        {
            Icon = new WindowIcon(image);
        }
        SetCurrentTitle();
    }
}
