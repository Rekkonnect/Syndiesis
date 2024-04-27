using Avalonia.Controls;

namespace Syndiesis.Utilities;

// Not the wisest naming choice
using ControlList = Avalonia.Controls.Controls;

public static class ControlsExtensions
{
    public static void AddIfNotContained(this ControlList controls, Control control)
    {
        if (!controls.Contains(control))
        {
            controls.Add(control);
        }
    }
}
