using Avalonia.Controls;
using Syndiesis.Core;

namespace Syndiesis.Controls;

public sealed class LanguageVersionDropDownItem : SelectableTextBlock
{
    private RoslynLanguageVersion _version;

    public RoslynLanguageVersion Version
    {
        get => _version;
        set
        {
            _version = value;
            Text = value.DisplayVersionNumber();
        }
    }

    public LanguageVersionDropDownItem(RoslynLanguageVersion version)
    {
        Version = version;
    }
}
