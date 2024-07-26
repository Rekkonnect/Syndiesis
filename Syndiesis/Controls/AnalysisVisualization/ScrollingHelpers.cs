using Avalonia.Input;

namespace Syndiesis.Controls.AnalysisVisualization;

public static class ScrollingHelpers
{
    public static void ApplyWheelScrolling(
        PointerWheelEventArgs e,
        double scrollMultiplier,
        VerticalScrollBar verticalScrollBar,
        HorizontalScrollBar horizontalScrollBar)
    {
        double steps = -e.Delta.Y * scrollMultiplier;
        double verticalSteps = steps;
        double horizontalSteps = -e.Delta.X * scrollMultiplier;
        if (horizontalSteps is 0)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                horizontalSteps = verticalSteps;
                verticalSteps = 0;
            }
        }

        verticalScrollBar.Step(verticalSteps);
        horizontalScrollBar.Step(horizontalSteps);
    }
}
