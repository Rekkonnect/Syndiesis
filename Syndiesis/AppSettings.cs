using Syndiesis.Controls.SyntaxVisualization.Creation;
using System;

namespace Syndiesis;

public sealed class AppSettings
{
    public static readonly AppSettings Instance = new();

    public NodeLineCreationOptions CreationOptions { get; set; } = new();

    public int ExtraBufferLines { get; set; } = 5;
    public TimeSpan UserInputDelay { get; set; } = TimeSpan.FromMinutes(300);
}
