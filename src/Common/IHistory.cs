namespace AspNetCoreHistory.Common;

/// <summary>
/// A type that has a history of changes.
/// </summary>
/// <typeparam name="TBase">The type of the entity itself.</typeparam>
/// <typeparam name="THistory">The type of the history.</typeparam>
/// <typeparam name="TKey">The type of the parent id.</typeparam>
public interface IHasHistory<TBase, THistory, TKey> 
    where THistory: IHistory<THistory, TBase, TKey>
    where TBase : IHasHistory<TBase, THistory, TKey>
{
}

/// <inheritdoc/>
public interface IHasHistory<TBase, THistory> : IHasHistory<TBase, THistory, int>
    where THistory : IHistory<THistory, TBase, int>
    where TBase : IHasHistory<TBase, THistory, int>
{
}

/// <summary>
/// Track the different changes of an entity.
/// </summary>
/// <typeparam name="TSelf">The type of the history itself.</typeparam>
/// <typeparam name="TBase">The type of the entity to track.</typeparam>
/// <typeparam name="TKey">The id of the parent.</typeparam>
public interface IHistory<TSelf, TBase, TKey>
    where TSelf: IHistory<TSelf, TBase, TKey>
    where TBase : IHasHistory<TBase, TSelf, TKey>
{
    /// <summary>
    /// Primary key of this history.
    /// </summary>
    public long HistoryId { get; set; }

    /// <summary>
    /// Id of the parent.
    /// </summary>
    public TKey Id { get; }

    /// <summary>
    /// The time when this new history version was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <inheritdoc/>
public interface IHistory<TSelf, TBase> : IHistory<TSelf, TBase, int> 
    where TSelf : IHistory<TSelf, TBase, int>
    where TBase : IHasHistory<TBase, TSelf, int>
{ }