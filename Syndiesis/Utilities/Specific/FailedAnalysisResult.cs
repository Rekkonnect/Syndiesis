using System;

namespace Syndiesis.Utilities.Specific;

public sealed class FailedAnalysisResult : AnalysisResult
{
    public FailedAnalysisResult(Exception exception)
    {
        Exception = exception;
    }
}
