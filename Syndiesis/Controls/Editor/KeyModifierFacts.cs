using Avalonia.Input;
using System;

namespace Syndiesis.Controls;

public static class KeyModifierFacts
{
    public static readonly KeyModifiers ControlKeyModifier
        = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
}
