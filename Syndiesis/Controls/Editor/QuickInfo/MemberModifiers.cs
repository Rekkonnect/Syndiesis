using Microsoft.CodeAnalysis;
using System;

namespace Syndiesis.Controls.Editor.QuickInfo;

/// <summary>
/// Represents a combination of modifiers that can be applied to various
/// members, including types and type members.
/// </summary>
/// <remarks>
/// This enum does not provide modifiers for accessibility other than
/// <see langword="file"/>. Check out <see cref="Accessibility"/> for
/// those.
/// </remarks>
[Flags]
public enum MemberModifiers
{
    None,
    
    File = 1,
    Async = 1 << 1,
    
    Sealed = 1 << 2,
    Override = 1 << 3,
    Abstract = 1 << 4,
    Virtual = 1 << 5,
    
    New = 1 << 6,
    ReadOnly = 1 << 7,
    Static = 1 << 8,
    Volatile = 1 << 9,
    FixedSizeBuffer = 1 << 10,
    Const = 1 << 11,
    Ref = 1 << 12,
    RefReadOnly = 1 << 13,
    Required = 1 << 14,
    Scoped = 1 << 15,
}
