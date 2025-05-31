using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Syndiesis.Core;

public static class RoslynInternalsEx
{
    public const string GetAnnotationsMethodName = "GetAnnotations";

    private static readonly Type _greenNodeType;
    private static readonly MethodInfo _getAnnotationsMethod;

    private static readonly GreenNodePropertyCache<SyntaxNode> _syntaxNodeGreen;
    private static readonly GreenNodePropertyCache<SyntaxToken> _syntaxTokenGreen;
    private static readonly GreenNodePropertyCache<SyntaxTrivia> _syntaxTriviaGreen;
    private static readonly GreenNodePropertyCache<SyntaxTriviaList> _syntaxTriviaListGreen;

    static RoslynInternalsEx()
    {
        var codeAnalysisAssembly = typeof(SyntaxNode);
        _greenNodeType = codeAnalysisAssembly.Assembly.GetType("Microsoft.CodeAnalysis.GreenNode")!;
        Debug.Assert(_greenNodeType is not null);

        const BindingFlags methodFlags = BindingFlags.Public | BindingFlags.Instance;
        _getAnnotationsMethod = _greenNodeType.GetMethod(GetAnnotationsMethodName, methodFlags, [])!;
        Debug.Assert(_getAnnotationsMethod is not null);

        GreenNodePropertyCache.DiscoverAssign(out _syntaxNodeGreen);
        GreenNodePropertyCache.DiscoverAssign(out _syntaxTokenGreen);
        GreenNodePropertyCache.DiscoverAssign(out _syntaxTriviaGreen);
        GreenNodePropertyCache.DiscoverAssign(out _syntaxTriviaListGreen);
    }

    /// <summary>
    /// Gets the syntax annotations for the given GreenNode, passed as an object.
    /// </summary>
    /// <param name="value">
    /// The value must be a GreenNode.
    /// </param>
    /// <returns>
    /// The available <seealso cref="SyntaxAnnotation"/> for the given green node.
    /// </returns>
    public static SyntaxAnnotation[] GetGreenNodeSyntaxAnnotations(object value)
    {
        Debug.Assert(value?.GetType().IsAssignableTo(_greenNodeType) ?? false);
        return (SyntaxAnnotation[])_getAnnotationsMethod.Invoke(value, [])!;
    }

    public static SyntaxAnnotation[] GetSyntaxAnnotations(SyntaxNode node)
    {
        var green = _syntaxNodeGreen.GetGreenNode(node);
        return GetGreenNodeSyntaxAnnotations(green);
    }

    public static SyntaxAnnotation[] GetSyntaxAnnotations(SyntaxToken token)
    {
        var green = _syntaxTokenGreen.GetGreenNode(token);
        return GetGreenNodeSyntaxAnnotations(green);
    }

    public static SyntaxAnnotation[] GetSyntaxAnnotations(SyntaxTrivia trivia)
    {
        var green = _syntaxTriviaGreen.GetGreenNode(trivia);
        return GetGreenNodeSyntaxAnnotations(green);
    }

    public static SyntaxAnnotation[] GetSyntaxAnnotations(SyntaxTriviaList triviaList)
    {
        var green = _syntaxTriviaListGreen.GetGreenNode(triviaList);
        return GetGreenNodeSyntaxAnnotations(green);
    }

    private static class GreenNodePropertyCache
    {
        public static void DiscoverAssign<T>(out GreenNodePropertyCache<T> field)
        {
            field = GreenNodePropertyCache<T>.Discover();
        }
    }

    private sealed record GreenNodePropertyCache<TType>(PropertyInfo GreenNodeProperty)
    {
        public object GetGreenNode(TType value)
        {
            return GreenNodeProperty.GetValue(value)!;
        }

        public static GreenNodePropertyCache<TType> Discover()
        {
            var type = typeof(TType);
            var property = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(t => t.PropertyType == _greenNodeType)
                .First();
            return new(property);
        }
    }
}
