using Microsoft.CodeAnalysis;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class IOperationPropertyFilterCache()
    : PublicApiInterfacePropertyFilterCache<IOperation>(IOperationPropertyFilter.Instance)
{
}
