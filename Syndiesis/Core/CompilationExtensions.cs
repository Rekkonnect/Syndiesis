using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Syndiesis.Core;

public static class CompilationExtensions
{
    public static TCompilation ModifyOptions<TCompilation>(
        this TCompilation compilation,
        Func<CompilationOptions, CompilationOptions> options)
        where TCompilation : Compilation
    {
        var newOptions = options(compilation.Options);
        return (TCompilation)compilation.WithOptions(newOptions);
    }
    
    public static Compilation ModifyOptions(
        this CSharpCompilation compilation,
        Func<CSharpCompilationOptions, CSharpCompilationOptions> options)
    {
        var newOptions = options(compilation.Options);
        return compilation.WithOptions(newOptions);
    }
    
    public static Compilation ModifyOptions(
        this VisualBasicCompilation compilation,
        Func<VisualBasicCompilationOptions, VisualBasicCompilationOptions> options)
    {
        var newOptions = options(compilation.Options);
        return compilation.WithOptions(newOptions);
    }
}
