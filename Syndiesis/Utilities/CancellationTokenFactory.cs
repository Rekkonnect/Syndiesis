using System.Threading;

namespace Syndiesis.Utilities;

public sealed class CancellationTokenFactory
{
    private CancellationTokenSource? _currentSource;

    public CancellationTokenSource CurrentSource
    {
        get
        {
            if (_currentSource is null)
            {
                return CreateSource();
            }

            if (_currentSource.IsCancellationRequested)
            {
                return CreateSource();
            }

            return _currentSource;
        }
    }

    public CancellationToken CurrentToken => CurrentSource.Token;

    public CancellationTokenSource CreateSource()
    {
        _currentSource = new CancellationTokenSource();
        return _currentSource;
    }

    public void Cancel()
    {
        _currentSource?.Cancel();
    }
}
