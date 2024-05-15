using Avalonia.Input;
using System;

namespace Syndiesis.Utilities;

public static class KeyModifierFacts
{
    public static readonly KeyModifiers ControlKeyModifier
        = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;

    /// <summary>
    /// Normalizes the key modifiers the common modifiers Shift, Alt and Control.
    /// This affects macOS where Meta is returned instead of Control.
    /// Other platforms are unaffected.
    /// </summary>
    public static KeyModifiers NormalizeByPlatform(this KeyModifiers modifiers)
    {
        var controlKey = ControlKeyModifier;
        if (controlKey is KeyModifiers.Control)
            return modifiers;

        if (modifiers.HasFlag(controlKey))
        {
            return modifiers & ~controlKey | KeyModifiers.Control;
        }

        return modifiers;
    }
}
