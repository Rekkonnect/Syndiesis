using AvaloniaEdit.Rendering;
using Syndiesis.Core;

namespace Syndiesis.Controls.Editor;

public abstract class RoslynColorizer(SingleTreeCompilationSource compilationSource)
    : DocumentColorizingTransformer
{
    public SingleTreeCompilationSource CompilationSource { get; } = compilationSource;

}
