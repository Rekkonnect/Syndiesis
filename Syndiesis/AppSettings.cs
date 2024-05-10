using Syndiesis.Controls.SyntaxVisualization.Creation;
using System;

namespace Syndiesis;

public sealed class AppSettings
{
    public static readonly AppSettings Instance = new();

    public NodeLineCreationOptions CreationOptions = new();
    public IndentationOptions IndentationOptions = new();

    public TimeSpan UserInputDelay = TimeSpan.FromMilliseconds(600);

    public bool EnableExpandingAllNodes = false;
}
