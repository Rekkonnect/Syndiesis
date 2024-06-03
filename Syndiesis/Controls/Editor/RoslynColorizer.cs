using AvaloniaEdit.Rendering;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor;

public abstract class RoslynColorizer(CSharpSingleTreeCompilationSource compilationSource)
    : DocumentColorizingTransformer
{
    public CSharpSingleTreeCompilationSource CompilationSource { get; } = compilationSource;

}
