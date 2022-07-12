namespace AspNetCoreHistory.Common;

// Just used to statically check field names
public static class HistoryPropertyNames
{
    class T1 : IHistory<T1, T2>
    {
        public int Id => throw new NotImplementedException();
        public long HistoryId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTime CreatedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    class T2 : IHasHistory<T2, T1> { }

    public const string HistoryId = nameof(IHistory<T1, T2>.HistoryId);
    public const string CreatedAt = nameof(IHistory<T1, T2>.CreatedAt);
}
