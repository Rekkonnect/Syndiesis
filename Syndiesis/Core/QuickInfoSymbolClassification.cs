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
    Dynamic,
    Array,
    Pointer,
    FunctionPointer,
    Extension,

    // Members
    Field,
    Property,
    Event,
    Method,
    Operator,
    Conversion,
    Constructor,
    EnumField,

    // Others
    Label,
    Local,
    RangeVariable,
    Discard,
    Parameter,
    TypeParameter,
    Constant,
    Preprocessing,
}