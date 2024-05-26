using System;
using System.Threading.Tasks;

namespace Syndiesis.Utilities;

// From Garyon with improvements
public sealed class AdvancedLazy<T>
{
    private T? _value;
    private readonly Func<T> _factory;

    public bool IsValueCreated { get; private set; }

    public T? ValueOrDefault => _value;

    public T Value
    {
        get
        {
            if (IsValueCreated)
                return _value!;

            IsValueCreated = true;
            return _value = _factory();
        }
    }

    public Func<T> Factory => _factory;

    public AdvancedLazy(T value)
        : this(() => value)
    {
        IsValueCreated = true;
        _value = value;
    }
    public AdvancedLazy(Func<T> factory)
    {
        _factory = factory;
    }

    public async Task<T> GetValueAsync()
    {
        if (IsValueCreated)
        {
            return _value!;
        }

        return await CalculateValueAsync();
    }

    private Task<T> CalculateValueAsync()
    {
        IsValueCreated = true;
        var value = _factory();
        _value = value;
        return Task.FromResult(value);
    }

    public void ClearValue()
    {
        IsValueCreated = false;
        _value = default;
    }
}
