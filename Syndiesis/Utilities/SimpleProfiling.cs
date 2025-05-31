namespace Syndiesis.Utilities;

public sealed class SimpleProfiling
{
    private Snapshot? _start;
    private Snapshot? _end;

    public Results? SnapshotResults { get; private set; }

    public void Begin()
    {
        _start = TakeSnapshot();
    }

    public void End()
    {
        _end = TakeSnapshot();
        SetResult();
    }

    public Process BeginProcess()
    {
        return new(this);
    }

    private void SetResult()
    {
        SnapshotResults = new()
        {
            Time = _end!.Time - _start!.Time,
            Memory = _end!.MemoryBytes - _start!.MemoryBytes,
        };
    }

    private Snapshot TakeSnapshot()
    {
        return new()
        {
            Time = DateTime.Now,
            MemoryBytes = GC.GetTotalMemory(false),
        };
    }

    public readonly struct Process : IDisposable
    {
        private readonly SimpleProfiling _profiling;

        public Process(SimpleProfiling profiling)
        {
            _profiling = profiling;
            profiling.Begin();
        }

        public void Dispose()
        {
            _profiling.End();
        }
    }

    private sealed class Snapshot
    {
        public required DateTime Time;
        public required long MemoryBytes;
    }

    public sealed class Results
    {
        public required TimeSpan Time { get; init; }
        public required long Memory { get; init; }
    }
}
