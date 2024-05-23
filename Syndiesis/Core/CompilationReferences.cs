using Microsoft.CodeAnalysis;
using RoseLynn;
using System.Collections.Generic;

namespace Syndiesis.Core;

public static class CompilationReferences
{
    public static readonly IReadOnlyList<MetadataReference> CurrentNetVersion =
    [
        MetadataReferenceFactory.CreateFromType<object>(),
        MetadataReferenceFactory.CreateFromType<System.Action>(),
        MetadataReferenceFactory.CreateFromType<System.Type>(),
    ];
}
