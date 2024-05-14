using System;

namespace Syndiesis.Core;

public sealed class FailedAnalysisResult : AnalysisResult
{
    public FailedAnalysisResult(Exception exception)
    {
        Exception = exception;
    }
}
