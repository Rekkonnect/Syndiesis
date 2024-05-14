using Serilog;
using System;

namespace Syndiesis;

public sealed class ExceptionListener
{
    public event Action<Exception>? ExceptionHandled;

    public void HandleException(Exception ex, string message)
    {
        Log.Logger.Error(ex, message);
        ExceptionHandled?.Invoke(ex);
    }
}
