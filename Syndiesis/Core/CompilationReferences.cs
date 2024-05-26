using Microsoft.CodeAnalysis;
using RoseLynn;
using System;
using System.Collections.Generic;

namespace Syndiesis.Core;

public static class CompilationReferences
{
    public static readonly IReadOnlyList<MetadataReference> CurrentNetVersion =
    [
        MetadataReferenceFactory.CreateFromType<object>(),
    ];

    public static readonly IReadOnlyList<MetadataReference> Runnable =
    [
        .. CurrentNetVersion,
        MetadataReferenceFactory.CreateFromType(typeof(Console)),
        MetadataReferenceFactory.CreateFromType(typeof(ISymbol)),
    ];
}
