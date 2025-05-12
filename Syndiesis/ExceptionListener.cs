using Serilog;
using System;

namespace Syndiesis;

public sealed class ExceptionListener
{
    public event Action<Exception>? ExceptionHandled;

    public void HandleException(Exception ex, string message)
    {
        if (ex is StackOverflowException or InsufficientExecutionStackException)
        {
            var replacement = new Exception($"An exception of type {ex} was thrown");
            ex = replacement;
        }
        Log.Logger.Error(ex, message);
        ExceptionHandled?.Invoke(ex);
    }
}
