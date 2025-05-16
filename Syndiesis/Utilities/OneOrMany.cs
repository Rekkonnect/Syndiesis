using System;
using System.Collections.Generic;

namespace Syndiesis.Utilities;

public readonly struct OneOrMany<T>
{
    private readonly Mode _mode;
    private readonly T? _single;
    private readonly IEnumerable<T>? _many;

    public Mode CollectionMode => _mode;

    /// <summary>
    /// Gets the single item that is stored in this collection.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the mode of this collection is not <see cref="Mode.One"/>.
    /// </exception>
    public T Single
    {
        get
        {
            if (_mode is not Mode.One)
            {
                throw new InvalidOperationException(
                    "Only invoke this property when the mode is Mode.One");
            }

            return _single!;
        }
    }

    public IEnumerable<T> Enumerable
    {
        get
        {
            return _mode switch
            {
                Mode.None => [],
                Mode.One => [_single!],
                Mode.Many => _many!,
                _ => [],
            };
        }
    }

    public OneOrMany()
    {
        _mode = Mode.None;
    }

    public OneOrMany(T single)
    {
        _single = single;
        _mode = Mode.One;
    }

    public OneOrMany(IEnumerable<T> many)
    {
        _many = many;
        _mode = Mode.Many;
    }

    public static OneOrMany<T> From<TSub>(OneOrMany<TSub> sub)
        where TSub : T
    {
        switch (sub._mode)
        {
            case Mode.None:
                return new();

            case Mode.One:
                return new(sub._single!);

            case Mode.Many:
                return new((IEnumerable<T>)sub._many!);

            default:
                throw new InvalidOperationException("Invalid mode");
        }
    }

    public static implicit operator OneOrMany<T>(T single)
    {
        return new(single);
    }

    public enum Mode
    {
        None,
        One,
        Many,
    }
}
