namespace Syndiesis.ColorHelpers;

public sealed class LazilyUpdatedHsvTransformedSolidBrush(
    ILazilyUpdatedSolidBrush mainSolid,
    HsvTransformation transformation)
    : LazilyUpdatedTransformedSolidBrush<HsvTransformation, HsvColor>(mainSolid, transformation)
{
}