namespace Syndiesis.Core.DisplayAnalysis;

public readonly record struct DisplayValueSource(
    DisplayValueSource.SymbolKind Kind, string? Name)
{
    public static readonly DisplayValueSource This = new(SymbolKind.This, string.Empty);
    public static readonly DisplayValueSource Indexer = new(SymbolKind.Indexer, string.Empty);

    public bool IsDefault
        => Kind is SymbolKind.None
        || Name is null;

    public static DisplayValueSource Property(string name)
        => new(SymbolKind.Property, name);

    public static DisplayValueSource Method(string name)
        => new(SymbolKind.Method, name);

    public enum SymbolKind
    {
        None = 0,
        Property = 1,
        Field = 2,
        Method = 3,
        Indexer = 4,
        This = 10,

        KindMask = 0xFF,

        FlagMask = ~KindMask,

        Async = 1 << 15,
        Internal = 1 << 16,
    }
}
