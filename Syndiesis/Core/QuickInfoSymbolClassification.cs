namespace Syndiesis.Core;

public enum QuickInfoSymbolClassification
{
    None,
    
    // Containers
    Namespace,
    Alias,
    Module,
    Assembly,
    
    // Types
    Class,
    Struct,
    Interface,
    Delegate,
    Enum,
    Error,
    
    // Members
    Field,
    Property,
    Event,
    Method,
    Operator,
    Conversion,
    EnumField,
    
    // Others
    Label,
    Local,
    Parameter,
    TypeParameter,
    Constant,
    Preprocessing,
}
