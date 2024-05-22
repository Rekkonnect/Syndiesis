using Garyon.DataStructures;
using Garyon.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Syndiesis.Core.DisplayAnalysis;

public static class TypeExtensions
{
    public static bool IsOrImplements(this Type interfaceType, Type target)
    {
        if (interfaceType == target)
            return true;

        if (interfaceType.GetInterface(target.Name) is not null)
            return true;

        return false;
    }

    // From Garyon, bugfixed
    public static Tree<Type> GetInterfaceInheritanceTree(this Type type)
    {
        var tree = new Tree<Type>(type);

        var leaves = new Queue<TreeNode<Type>>();
        leaves.Enqueue(tree.Root);

        if (!type.CanInheritInterfaces())
            return tree;

        while (leaves.Any())
        {
            var node = leaves.Dequeue();
            var nodeType = node.Value;

            // Add the interfaces
            var interfaces = nodeType.GetInterfaces();
            foreach (var i in interfaces)
            {
                var interfaceNode = node.AddChild(i);
                leaves.Enqueue(interfaceNode);

                // Remove indirectly inherited interfaces
                var currentParent = node.Parent;
                while (currentParent is not null)
                {
                    currentParent.RemoveChild(i);
                    currentParent = currentParent.Parent;
                }
            }
        }

        return tree;
    }
}
