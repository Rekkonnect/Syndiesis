using CSharpSyntaxEditor.Utilities;

namespace CSharpSyntaxEditor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public readonly MultilineStringEditor Editor = new();
}
