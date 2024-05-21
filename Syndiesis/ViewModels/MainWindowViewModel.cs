using Syndiesis.Core;

namespace Syndiesis.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public readonly CursoredStringEditor Editor = new();

    public readonly SingleTreeCompilationSource CompilationSource = new();
}
