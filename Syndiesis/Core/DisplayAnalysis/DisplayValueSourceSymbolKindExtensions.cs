namespace Syndiesis.Core.DisplayAnalysis;

public static class DisplayValueSourceSymbolKindExtensions
{
    public static bool IsAsync(this DisplayValueSource.SymbolKind kind)
    {
        return kind.HasFlag(DisplayValueSource.SymbolKind.Async);
    }
    public static bool IsInternal(this DisplayValueSource.SymbolKind kind)
    {
        return kind.HasFlag(DisplayValueSource.SymbolKind.Internal);
    }

    public static DisplayValueSource.SymbolKind RawKindWithoutFlags(
        this DisplayValueSource.SymbolKind kind)
    {
        return kind & DisplayValueSource.SymbolKind.KindMask;
    }
}
