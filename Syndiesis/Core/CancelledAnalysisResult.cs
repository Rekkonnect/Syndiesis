namespace Syndiesis.Core;

public sealed class CancelledAnalysisResult : AnalysisResult
{
    public override bool Cancelled => true;
}
