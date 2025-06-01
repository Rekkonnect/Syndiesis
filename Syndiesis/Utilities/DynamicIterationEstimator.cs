namespace Syndiesis.Utilities;

public sealed class DynamicIterationEstimator(TimeSpan maxAllocatedTime)
{
    private readonly LifoBuffer<double> _iterationTimes = new(20);

    public TimeSpan MaxAllocatedTime { get; set; } = maxAllocatedTime;
    public int RecommendedIterationCount { get; private set; }

    private DateTime _beginTime;

    public void Begin()
    {
        _beginTime = DateTime.Now;
    }

    public void End(int executedIterations)
    {
        var end = DateTime.Now;
        var elapsed = end - _beginTime;
        var iterationTime = elapsed.TotalMilliseconds / executedIterations;
        _iterationTimes.Append(iterationTime);

        var iterations = MaxAllocatedTime.TotalMilliseconds / GeometricMean(_iterationTimes.GetBuffer());
        RecommendedIterationCount = (int)iterations;
    }

    public Process BeginProcess(int iterationCount)
    {
        return new(this, iterationCount);
    }

    private static double GeometricMean(ReadOnlySpan<double> values)
    {
        double product = 1;
        foreach (var value in values)
        {
            product *= value;
        }

        return Math.Pow(product, 1D / values.Length);
    }

    public readonly struct Process
        : IDisposable
    {
        private readonly DynamicIterationEstimator _estimator;
        private readonly int _iterationCount;

        public Process(DynamicIterationEstimator estimator, int iterationCount)
        {
            _estimator = estimator;
            _iterationCount = iterationCount;
            _estimator.Begin();
        }

        public void Dispose()
        {
            _estimator.End(_iterationCount);
        }
    }

    private sealed class LifoBuffer<T>
    {
        private readonly T[] _buffer;
        private int _appends;

        public LifoBuffer(int capacity)
        {
            _buffer = new T[capacity];
            _appends = 0;
        }

        public void Append(T value)
        {
            _buffer[_appends % _buffer.Length] = value;
            _appends++;
        }

        public ReadOnlySpan<T> GetBuffer()
        {
            int length = Math.Min(_appends, _buffer.Length);
            return _buffer.AsSpan()[..length];
        }
    }
}
