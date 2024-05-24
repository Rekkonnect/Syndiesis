using System;

namespace Syndiesis.InternalGenerators.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class SolidColorAttribute(string name, uint defaultColorValue)
    : Attribute
{
    public string Name { get; } = name;
    public uint DefaultColorValue { get; } = defaultColorValue;
}
