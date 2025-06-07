using Avalonia.Controls.ApplicationLifetimes;

namespace Syndiesis;

public static class ApplicationLifetimeExtensions
{
    public static Visual? GetTopLevelVisual(
        this IApplicationLifetime lifetime)
    {
        return lifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime view => view.MainView,
            _ => null,
        };
    }
}
