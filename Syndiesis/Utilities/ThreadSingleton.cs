using System;

namespace Syndiesis.Utilities;

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
