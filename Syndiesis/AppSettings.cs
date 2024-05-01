using Syndiesis.Controls.SyntaxVisualization.Creation;
using System;

namespace Syndiesis;

public sealed class AppSettings
{
    public static readonly AppSettings Instance = new();

    public NodeLineCreationOptions CreationOptions { get; set; } = new();

    public TimeSpan UserInputDelay { get; set; } = TimeSpan.FromMilliseconds(600);

    public bool EnableExpandingAllNodes { get; set; } = false;
}
