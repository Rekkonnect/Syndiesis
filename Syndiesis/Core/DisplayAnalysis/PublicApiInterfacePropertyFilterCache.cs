using Garyon.Extensions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

public abstract class PublicApiInterfacePropertyFilterCache<TInterface>(PropertyFilter propertyFilter)
    : InterestingPropertyFilterCache(propertyFilter)
{
    public override PropertyFilterResult FilterForType(Type type)
    {
        // We won't run into this case very often
        if (type.IsInterface)
        {
            return base.FilterForType(type);
        }

        var interfaces = FilterInterfaces(type);

        var directInterfaces = type.GetInterfaceInheritanceTree()
            .Root.Children.Select(s => s.Value);

        // We want to make sure that there is only one operation interface at most,
        // so we allow the exception
        var filteredInterface = directInterfaces.SingleOrDefault(IsFilteredInterface);

        if (filteredInterface is null)
            return PropertyFilterResult.Empty;

        var filteredInterfaces = filteredInterface.GetInterfaces()
            .ConcatSingleValue(filteredInterface);
        var properties = filteredInterfaces.SelectMany(
            @interface => base.FilterForType(@interface).Properties);

        return new()
        {
            Properties = properties.ToList(),
            PreferredType = filteredInterface
        };
    }

    private static IEnumerable<Type> FilterInterfaces(Type concreteType)
    {
        return concreteType.GetInterfaces()
            .Where(IsFilteredInterface)
            ;
    }

    private static bool IsFilteredInterface(Type type)
    {
        return type.IsOrImplements(typeof(TInterface));
    }
}
