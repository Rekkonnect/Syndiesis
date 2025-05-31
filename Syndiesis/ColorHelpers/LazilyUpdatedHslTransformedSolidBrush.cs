namespace Syndiesis.ColorHelpers;

public sealed class LazilyUpdatedHslTransformedSolidBrush(
    LazilyUpdatedSolidBrush mainSolid,
    HslTransformation transformation)
    : LazilyUpdatedTransformedSolidBrush<HslTransformation, HslColor>(mainSolid, transformation)
{
}
