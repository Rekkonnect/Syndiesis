using System;

namespace Syndiesis.Utilities;

public sealed class RateLimiter(TimeSpan minOffset)
{
    private DateTime _last;

    public TimeSpan MinOffset = minOffset;

    public bool Request()
    {
        var now = DateTime.Now;
        var offset = now - _last;
        if (offset < MinOffset)
            return false;

        _last = now;
        return true;
    }
}
