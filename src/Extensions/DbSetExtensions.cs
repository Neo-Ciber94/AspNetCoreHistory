using AspNetCoreHistory.Common;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreHistory.Extensions;

public static class DbSetExtensions
{
    public static IEnumerable<VersionedEntity<THistory>> GetVersions<THistory, TBase, TKey>(this DbSet<THistory> self)
        where THistory : class, IHistory<THistory, TBase, TKey>
        where TBase : IHasHistory<TBase, THistory, TKey>, IEntity<TKey>
    {
        var result = self
            .ToList()
            .GroupBy(x => x.Id)
            .SelectMany(x => x.OrderBy(e => e.CreatedAt).Select((e, i) => new VersionedEntity<THistory>(e, i + 1)));

        return result;
    }

    public static IEnumerable<VersionedEntity<THistory>> GetVersions<THistory, TBase>(this DbSet<THistory> self)
        where THistory : class, IHistory<THistory, TBase, int>
        where TBase : IHasHistory<TBase, THistory, int>, IEntity<int>
    {
        return GetVersions<THistory, TBase, int>(self);
    }

    public static IQueryable<VersionedEntity<THistory>> FindVersions<THistory, TBase, TKey>(this DbSet<THistory> self, TKey id) 
        where THistory: class, IHistory<THistory, TBase, TKey>
        where TBase : IHasHistory<TBase, THistory, TKey>, IEntity<TKey>
    {
        return self
            .Where(x => x.Id!.Equals(id))
            .OrderBy(x => x.CreatedAt)
            .Select((x, i) => new VersionedEntity<THistory>(x, i + 1));
    }

    public static IQueryable<VersionedEntity<THistory>> FindVersions<THistory, TBase>(this DbSet<THistory> self, int id)
    where THistory : class, IHistory<THistory, TBase, int>
    where TBase : IHasHistory<TBase, THistory, int>, IEntity<int>
    {
        return FindVersions<THistory, TBase, int>(self, id);
    }
}
