using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Syndiesis.Core.DisplayAnalysis;

using ReadOnlySyntaxNodeList = IReadOnlyList<SyntaxNode>;

public sealed class SyntaxNodePropertyFilter : PropertyFilter
{
    public static readonly SyntaxNodePropertyFilter Instance = new();

    public override PropertyFilterResult FilterProperties(Type type)
    {
        var properties = type.GetProperties();
        var interestingTypeProperties = properties.Where(FilterNodeProperty)
            .ToArray();

        return new()
        {
            Properties = interestingTypeProperties
        };
    }

    private static bool FilterNodeProperty(PropertyInfo propertyInfo)
    {
        var name = propertyInfo.Name;

        // we don't like infinite recursion
        switch (name)
        {
            case nameof(SyntaxNode.Parent):
            case nameof(StructuredTriviaSyntax.ParentTrivia):
                return false;
        }

        bool extraFilter = IsExtraProperty(propertyInfo, name);
        if (extraFilter)
            return false;

        var type = propertyInfo.PropertyType;

        if (type.IsGenericType)
        {
            var interfaces = type.GetInterfaces();
            bool isListOfSyntaxNodes = interfaces.Any(
                i => i.IsAssignableTo(typeof(ReadOnlySyntaxNodeList)));
            return isListOfSyntaxNodes;
        }

        if (IsSyntaxNodeType(type))
            return true;

        return type == typeof(SyntaxToken)
            || type == typeof(SyntaxTokenList)
            || type == typeof(SyntaxTrivia)
            || type == typeof(SyntaxTriviaList)
            ;
    }

    private static bool IsExtraProperty(PropertyInfo propertyInfo, string name)
    {
        if (name is nameof(UsingDirectiveSyntax.Name))
        {
            if (propertyInfo.DeclaringType == typeof(UsingDirectiveSyntax))
                return true;
        }

        if (name is nameof(DirectiveTriviaSyntax.DirectiveNameToken))
        {
            if (propertyInfo.DeclaringType == typeof(DirectiveTriviaSyntax))
                return true;
        }

        return false;
    }

    private static bool IsSyntaxNodeType(Type type)
    {
        var current = type;
        while (true)
        {
            if (current is null)
                return false;

            if (current == typeof(SyntaxNode))
                return true;

            current = current.BaseType;
        }
    }
}
