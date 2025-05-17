using Avalonia.Media;

namespace Syndiesis;

// TODO Move to .Colors
public interface ILazilyUpdatedSolidBrush : ILazilyUpdatedBrush
{
    public Color Color { get; }
}
