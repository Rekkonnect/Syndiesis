using AvaloniaEdit.Document;
using Syndiesis.Core;
using System;

namespace Syndiesis.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public readonly TextDocument Document = new();

    public readonly HybridSingleTreeCompilationSource HybridCompilationSource = new();

    public ISingleTreeCompilationSource CompilationSource => HybridCompilationSource.CurrentSource;
}
