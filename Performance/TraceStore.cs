public static class TraceStore
{
    private static readonly List<ApiTraceResult> _traces = new();

    private const int MaxItems = 200; // prevent unbounded memory use

    public static void Add(ApiTraceResult trace)
    {
        lock (_traces)
        {
            _traces.Add(trace);

            if (_traces.Count > MaxItems)
                _traces.RemoveAt(0);
        }
    }

    public static IReadOnlyList<ApiTraceResult> GetAll()
    {
        lock (_traces)
        {
            return _traces.ToList();
        }
    }

    public static void Clear()
    {
        lock (_traces)
        {
            _traces.Clear();
        }
    }
}
