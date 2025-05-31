using Garyon.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using RoseLynn;
using System.Reflection;

namespace Syndiesis.Core;

public static class CompilationReferences
{
    public static readonly IReadOnlyList<MetadataReference> CurrentNetVersion =
    [
        MetadataReferenceFactory.CreateFromType<object>(),
    ];

    public static readonly IReadOnlyList<MetadataReference> Runnable;

    static CompilationReferences()
    {
        var runnableAssemblies = new AssemblyCollection();

        runnableAssemblies.AddReferences(CurrentNetVersion);
        runnableAssemblies.AddReference(MetadataReferenceFactory.CreateFromType(typeof(Console)));
        runnableAssemblies.AddTransitively(typeof(GeneratorAttribute).Assembly);
        runnableAssemblies.AddTransitively(typeof(CSharpSyntaxTree).Assembly);
        runnableAssemblies.AddTransitively(typeof(VisualBasicSyntaxTree).Assembly);
        var references = runnableAssemblies.CreateReferences();
        Runnable = references.ToArray();
    }

    private sealed class AssemblyCollection
    {
        private readonly HashSet<AssemblyName> _names = new();
        private readonly HashSet<Assembly> _assemblies = new();
        private readonly HashSet<MetadataReference> _references = new();

        public IEnumerable<MetadataReference> CreateReferences()
        {
            return
            [
                .. _assemblies.Select(MetadataReferenceFactory.CreateFromAssembly),
                .. _names.Select(CreateFromName),
                .. _references,
            ];
        }

        private static MetadataReference CreateFromName(AssemblyName name)
        {
            return MetadataReferenceFactory.CreateFromAssembly(Assembly.Load(name));
        }

        public void AddReferences(IEnumerable<MetadataReference> references)
        {
            _references.AddRange(references);
        }

        public void AddReference(MetadataReference reference)
        {
            _references.Add(reference);
        }

        public void AddTransitively(Assembly assembly)
        {
            bool added = _assemblies.Add(assembly);
            if (!added)
                return;

            var references = assembly.GetReferencedAssemblies();

            foreach (var reference in references)
            {
                var referencedAssembly = Assembly.Load(reference);
                if (!_assemblies.Contains(referencedAssembly))
                {
                    AddTransitively(referencedAssembly);
                }
            }
        }
    }
}
