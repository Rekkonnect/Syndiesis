using Microsoft.CodeAnalysis;
using RoseLynn;

namespace Syndiesis.Controls.Editor;

public static class SymbolHelpers
{
    public static class CSharp
    {
        public static string? KeywordForAccessor(IMethodSymbol method)
        {
            if (method.IsInitOnly)
            {
                return "init";
            }

            return method.MethodKind switch
            {
                MethodKind.PropertyGet => "get",
                MethodKind.PropertySet => "set",
                MethodKind.EventAdd => "add",
                MethodKind.EventRemove => "remove",
                _ => null,
            };
        }

        public static string ConversionOperatorImplicationModeKeyword(OperatorKind kind)
        {
            return kind switch
            {
                OperatorKind.Implicit => "implicit",
                OperatorKind.Explicit => "explicit",
                _ => "unknown",
            };
        }
    }
}
