using Avalonia.Media;

namespace Syndiesis.ColorHelpers;

public sealed class LazilyUpdatedHsvTransformedSolidBrush(
    LazilyUpdatedSolidBrush mainSolid,
    HsvTransformation transformation)
    : LazilyUpdatedTransformedSolidBrush<HsvTransformation, HsvColor>(mainSolid, transformation)
{
}