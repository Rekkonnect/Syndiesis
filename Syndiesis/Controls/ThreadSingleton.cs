using System;

namespace Syndiesis.Controls;

public sealed class ThreadSingleton<T>
    where T : new()
{
    [ThreadStatic]
    public static readonly T Instance;

    static ThreadSingleton()
    {
        Instance = new();
    }
}
