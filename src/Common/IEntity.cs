namespace AspNetCoreHistory.Common;

public interface IEntity<TKey>
{
    public TKey Id { get; set; }
}


public interface IEntity : IEntity<int> { }