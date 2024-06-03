using AvaloniaEdit.Document;
using Syndiesis.Core;

namespace Syndiesis.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public readonly TextDocument Document = new();

    public readonly CSharpSingleTreeCompilationSource CompilationSource = new();
}
