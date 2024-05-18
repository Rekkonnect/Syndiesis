using Garyon.Extensions;
using Garyon.Reflection;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

public sealed class IOperationPropertyFilterCache()
    : InterestingPropertyFilterCache(IOperationPropertyFilter.Instance)
{
    public override PropertyFilterResult FilterForType(Type type)
    {
        // We won't run into this case very often
        if (type.IsInterface)
        {
            return base.FilterForType(type);
        }

        var interfaces = OperationInterfaces(type);

        var directInterfaces = type.GetInterfaceInheritanceTree()
            .Root.Children.Select(s => s.Value);

        // We want to make sure that there is only one operation interface at most,
        // so we allow the exception
        var operationInterface = directInterfaces.SingleOrDefault(IsOperationInterface);

        if (operationInterface is null)
            return PropertyFilterResult.Empty;

        var operationInterfaces = operationInterface.GetInterfaces()
            .ConcatSingleValue(operationInterface);
        var properties = operationInterfaces.SelectMany(
            @interface => base.FilterForType(@interface).Properties);

        return new()
        {
            Properties = properties.ToList(),
            PreferredType = operationInterface
        };
    }

    private static IEnumerable<Type> OperationInterfaces(Type operationType)
    {
        return operationType.GetInterfaces()
            .Where(IsOperationInterface)
            ;
    }

    private static bool IsOperationInterface(Type type)
    {
        return type.IsOrImplements(typeof(IOperation));
    }
}
