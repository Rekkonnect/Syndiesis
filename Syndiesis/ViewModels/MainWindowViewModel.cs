using AvaloniaEdit.Document;
using Syndiesis.Core;

namespace Syndiesis.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public readonly TextDocument Document = new();

    public readonly HybridSingleTreeCompilationSource HybridCompilationSource = new();

    public ISingleTreeCompilationSource CompilationSource => HybridCompilationSource.CurrentSource;

    public string CurrentLanguage => CompilationSource.LanguageName;
}
