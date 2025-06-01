using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using System.Collections.Immutable;

namespace Syndiesis.Core;

public sealed class AttributeDataViewModel
{
    public required AttributeData AttributeData { get; init; }
    public required ImmutableArray<LinkedAttributeArgument> ConstructorArguments { get; init; }
    public required ImmutableArray<LinkedAttributeArgument> NamedArguments { get; init; }

    public ImmutableArray<LinkedAttributeArgument> AllArguments
    {
        get
        {
            return
            [
                .. ConstructorArguments,
                .. NamedArguments,
            ];
        }
    }

    private AttributeDataViewModel() { }

    public static AttributeDataViewModel? Create(
        AttributeData data,
        CancellationToken cancellationToken = default)
    {
        var syntax = data.ApplicationSyntaxReference?.GetSyntax(cancellationToken);
        if (syntax is null)
            return null;

        if (data.HasNoArguments())
            return Empty(data);

        switch (syntax)
        {
            case CSharpSyntaxNode csAttribute:
                return CreateCSharpModel(data, csAttribute);
            case VisualBasicSyntaxNode vbAttribute:
                return CreateVisualBasicModel(data, vbAttribute);
            default:
                return null;
        }
    }

    private static AttributeDataViewModel Empty(AttributeData data)
    {
        return new()
        {
            AttributeData = data,
            ConstructorArguments = [],
            NamedArguments = [],
        };
    }

    private static AttributeDataViewModel? CreateCSharpModel(
        AttributeData data, CSharpSyntaxNode csAttribute)
    {
        var attribute = csAttribute as Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax;
        if (attribute is null)
            return null;

        var argumentList = attribute.ArgumentList;
        if (argumentList is null)
            return Empty(data);

        var arguments = argumentList.Arguments;
        if (arguments is [])
            return Empty(data);

        return ConstructFromArguments(data, arguments);
    }

    private static AttributeDataViewModel? CreateVisualBasicModel(
        AttributeData data, VisualBasicSyntaxNode vbAttribute)
    {
        var attribute = vbAttribute as Microsoft.CodeAnalysis.VisualBasic.Syntax.AttributeSyntax;
        if (attribute is null)
            return null;

        var argumentList = attribute.ArgumentList;
        if (argumentList is null)
            return Empty(data);

        var arguments = argumentList.Arguments;
        if (arguments is [])
            return Empty(data);

        return ConstructFromArguments(data, arguments);
    }

    private static AttributeDataViewModel ConstructFromArguments<TSyntax>(
        AttributeData data, SeparatedSyntaxList<TSyntax> arguments)
        where TSyntax : SyntaxNode
    {
        ImmutableArray<LinkedAttributeArgument> regularArguments = [];
        ImmutableArray<LinkedAttributeArgument> namedArguments = [];

        if (data.ConstructorArguments is not [] and var constructorArgumentsData)
        {
            var regularArgumentsBuilder = ImmutableArray.CreateBuilder<LinkedAttributeArgument>(
                constructorArgumentsData.Length);

            // We always have a constructor if we have constructor arguments
            // We expect a potential breaking API change that returns a null
            // constructor with a non-empty argument list, in which case we show the param order
            var constructor = data.AttributeConstructor;
            var constructorParameters = constructor?.Parameters;
            var mappingKind = 
                constructorParameters is not null
                ? AttributeArgumentNameMappingKind.Parameter
                : AttributeArgumentNameMappingKind.ParameterIndex;
            for (int i = 0; i < constructorArgumentsData.Length; i++)
            {
                var argumentSyntax = arguments[i];
                var value = constructorArgumentsData[i];
                var parameter = constructorParameters?[i];
                var displayName = parameter?.Name ?? i.ToString();
                regularArgumentsBuilder.Add(new(
                    argumentSyntax,
                    displayName,
                    value,
                    mappingKind));
            }
            regularArguments = regularArgumentsBuilder.ToImmutable();
        }

        if (data.NamedArguments is not [] and var namedArgumentsData)
        {
            var namedArgumentsBuilder = ImmutableArray.CreateBuilder<LinkedAttributeArgument>(
                namedArgumentsData.Length);

            int offset = data.ConstructorArguments.Length;

            for (int i = 0; i < namedArgumentsData.Length; i++)
            {
                var argumentSyntax = arguments[i + offset];
                var kvp = namedArgumentsData[i];
                var name = kvp.Key;
                var value = kvp.Value;
                namedArgumentsBuilder.Add(new(
                    argumentSyntax,
                    name,
                    value,
                    AttributeArgumentNameMappingKind.Named));
            }
            namedArguments = namedArgumentsBuilder.ToImmutable();
        }

        return new()
        {
            AttributeData = data,
            ConstructorArguments = regularArguments,
            NamedArguments = namedArguments,
        };
    }

    public sealed record class LinkedAttributeArgument(
        SyntaxNode ArgumentSyntax,
        string Name,
        TypedConstant Value,
        AttributeArgumentNameMappingKind MappingKind)
        ;

    public enum AttributeArgumentNameMappingKind
    {
        Parameter,
        ParameterIndex,
        Named,
    }
}
