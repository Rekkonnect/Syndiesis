using Dentextist;
using Microsoft.CodeAnalysis;
using RoseLynn;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Syndiesis.InternalGenerators;

[Generator]
public class SolidColorFieldGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Syndiesis.InternalGenerators.Core.SolidColorAttribute",
            Filter,
            Transform)
            ;

        context.RegisterSourceOutput(provider, GenerateSource);
    }

    private void GenerateSource(
        SourceProductionContext context,
        ImmutableArray<SolidColorAttributeData> data)
    {
        var groups = data
            .GroupBy<SolidColorAttributeData, INamedTypeSymbol>(
                d => d.TargetType,
                SymbolEqualityComparer.Default);

        foreach (var group in groups)
        {
            var writer = new Writer();
            writer.AppendHeaderText();

            var scope = writer.AppendPartialType(group.Key);

            bool first = true;
            foreach (var field in group)
            {
                writer.AppendField(field, !first);
                first = false;
            }

            scope.Dispose();

            var result = writer.ToString();
            context.AddSource($"{group.Key.ToDisplayString()}.SolidColorFields.g.cs", result);
        }
    }

    private ImmutableArray<SolidColorAttributeData> Transform(
        GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        var target = context.TargetSymbol as INamedTypeSymbol;
        Debug.Assert(target is not null);

        return context.Attributes
            .Select((data) => SolidColorAttributeData.Parse(target!, data))
            .ToImmutableArray()
            ;
    }

    private static bool Filter(SyntaxNode node, CancellationToken token)
    {
        return true;
    }

    private sealed class Writer : IndentedStringBuilder
    {
        public Writer()
            : base(' ', 4)
        {
        }

        public void AppendHeaderText()
        {
            const string header = """
                using Color = Avalonia.Media.Color;
                using SolidColorBrush = Avalonia.Media.SolidColorBrush;


                """;

            AppendLine(header);
        }

        public TypeScope AppendPartialType(INamedTypeSymbol type)
        {
            var scope = new TypeScope(this, type);
            return scope;
        }

        public void AppendField(SolidColorAttributeData data, bool includeNewLineAbove)
        {
            if (includeNewLineAbove)
            {
                AppendLine();
            }

            var valueString = data.DefaultColorValue.ToString("X8");
            AppendLine($"public const uint {data.Name}DefaultInt = 0x{valueString};");
            AppendLine($"public Color {data.Name}Color = Color.FromUInt32({data.Name}DefaultInt);");
            AppendLine($"public SolidColorBrush {data.Name}Brush = new({data.Name}DefaultInt);");
        }

        public sealed class TypeScope : IDisposable
        {
            private readonly IndentedStringBuilder _builder;
            private int _nestingLevels;

            public TypeScope(IndentedStringBuilder builder, INamedTypeSymbol type)
            {
                _builder = builder;
                var initialNesting = builder.NestingLevel;
                InitializeForType(type);
                _nestingLevels = builder.NestingLevel - initialNesting;
            }

            private void InitializeForType(INamedTypeSymbol type)
            {
                var containingType = type.ContainingType;
                if (containingType is not null)
                {
                    InitializeForType(containingType);
                }
                else
                {
                    var containingNamespace = type.ContainingNamespace;
                    WriteNamespace(containingNamespace);
                }

                WriteType(type);
            }

            private void WriteNamespace(INamespaceSymbol @namespace)
            {
                if (@namespace.IsGlobalNamespace)
                    return;

                _builder.AppendLine($"namespace {@namespace.ToDisplayString()}");
                _builder.AppendLine('{');
                _builder.IncrementNestingLevel();
            }

            private void WriteType(INamedTypeSymbol type)
            {
                var identifiableKind = type.GetIdentifiableSymbolKind();
                var typeKind = identifiableKind switch
                {
                    IdentifiableSymbolKind.Class => "class",
                    IdentifiableSymbolKind.Struct => "struct",
                    IdentifiableSymbolKind.Interface => "interface",
                    IdentifiableSymbolKind.RecordClass => "record class",
                    IdentifiableSymbolKind.RecordStruct => "record struct",
                    _ => throw new InvalidOperationException("Unsupported type kind."),
                };
                // Generic types are not supported
                _builder.AppendLine($"partial {typeKind} {type.Name}");
                _builder.AppendLine('{');
                _builder.IncrementNestingLevel();
            }

            public void Dispose()
            {
                for (int i = 0; i < _nestingLevels; i++)
                {
                    _builder.NestingLevel--;
                    _builder.AppendLine('}');
                }
            }
        }
    }

    private record SolidColorAttributeData(
        INamedTypeSymbol TargetType,
        string Name,
        uint DefaultColorValue)
    {
        public static SolidColorAttributeData Parse(
            INamedTypeSymbol targetType, AttributeData attributeData)
        {
            var name = attributeData.ConstructorArguments[0].Value as string
                ?? throw new InvalidOperationException("Name argument is required.");

            var defaultColorValue = (uint)attributeData.ConstructorArguments[1].Value!;

            return new SolidColorAttributeData(targetType, name, defaultColorValue);
        }
    }
}
