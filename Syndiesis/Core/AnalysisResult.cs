using System;

namespace Syndiesis.Core;

public abstract class AnalysisResult
{
    public Exception? Exception { get; set; }

    public bool Failed => Exception is not null;

    public virtual bool Cancelled => false;
}
