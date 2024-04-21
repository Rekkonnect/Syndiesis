using Avalonia.Media;

namespace CSharpSyntaxEditor.Models;

/// <summary>
/// Represents a highlighting range within a line.
/// </summary>
/// <param name="Start">The inclusive zero-based start index of the range.</param>
/// <param name="End">The exclusive zero-based end index of the range.</param>
/// <param name="Highlight">The color to highlight the span with.</param>
public readonly record struct LineHighlightRange(int Start, int End, Color Highlight);
