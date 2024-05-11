using System;

namespace Syndiesis.Controls;

public sealed class Singleton<T>
    where T : new()
{
    public static readonly T Instance = new();
}
