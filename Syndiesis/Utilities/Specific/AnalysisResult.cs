using System;

namespace Syndiesis.Utilities.Specific;

public abstract class AnalysisResult
{
    public Exception? Exception { get; set; }

    public bool Failed => Exception is not null;
}
