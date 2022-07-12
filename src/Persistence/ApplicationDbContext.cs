using AspNetCoreHistory.Common;
using AspNetCoreHistory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AspNetCoreHistory.Persistence;

public class ApplicationDbContext : DbContext
{
    record EntityEntryReference(
        EntityEntry Value,
        PropertyValues CurrentValues,
        PropertyValues OriginalValues,
        EntityState State);

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductHistory> ProductHistories => Set<ProductHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureHistoryEntities(modelBuilder);
    }

    private static void ConfigureHistoryEntities(ModelBuilder modelBuilder)
    {
        var historyTypes = modelBuilder.Model
            .GetEntityTypes()
            .Where(et =>
            {
                var type = et.ClrType;
                return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHistory<,>));
            });

        foreach (var entityType in historyTypes)
        {
            var entity = modelBuilder.Entity(entityType.ClrType);
            entity.HasKey(nameof(HistoryPropertyNames.HistoryId));

            var type = entityType.ClrType;
            var historyInterface = type
                .GetInterfaces()
                .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHistory<,>));

            var baseType = historyInterface.GetGenericArguments()[1]; // IHistory<TSelf, TBase> 
            var baseProperties = modelBuilder.Model
                .GetEntityTypes()
                .First(x => x.ClrType == baseType)
                .GetProperties();

            var historyProperties = type.GetProperties().ToDictionary(x => x.Name);

            // Ensure we are matching all parent types
            foreach (var property in baseProperties)
            {

                if (!historyProperties.TryGetValue(property.Name, out var prop) || prop.PropertyType != property.ClrType)
                {
                    throw new InvalidOperationException($"'{type.Name}' do not declare the parent property '{property.ClrType.Name} {property.Name}'");
                }
            }
        }
    }

    private IEnumerable<EntityEntryReference> GetAddedModifiedOrDeletedEntries()
    {
        return ChangeTracker
            .Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Added)
            .Select(e => new EntityEntryReference(e, e.CurrentValues.Clone(), e.OriginalValues.Clone(), e.State))
            .ToList();
    }

    public override int SaveChanges()
    {
        var entries = GetAddedModifiedOrDeletedEntries();
        var result = base.SaveChanges();
        SaveHistory(entries);
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = GetAddedModifiedOrDeletedEntries();
        var result = await base.SaveChangesAsync(cancellationToken);
        SaveHistory(entries);
        return result;
    }

    private void SaveHistory(IEnumerable<EntityEntryReference> entityEntries)
    {
        var entriesWithHistory = entityEntries.Where(e =>
        {
            var type = e.Value.Entity.GetType();
            return Enumerable.Any(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHasHistory<,>));
        });

        var newHistoryEntries = new List<object>();

        foreach (var entry in entriesWithHistory)
        {
            var state = entry.State;
            var historyEntry = CreateNewHistoryVersion(entry);

            if (historyEntry != null)
            {
                newHistoryEntries.Add(historyEntry);
            }
        }

        if (newHistoryEntries.Count > 0)
        {
            // Save the new versions
            AddRange(newHistoryEntries);
            SaveChanges();
        }
    }

    private static object? CreateNewHistoryVersion(EntityEntryReference entryReference)
    {
        var entry = entryReference.Value;

        // Ensure there are changes before add a new version
        if (!HasChanges(entryReference))
        {
            return null;
        }

        var entity = entry.Entity;
        var type = entity.GetType();
        var hasHistoryInterface = type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHasHistory<,>))
            .Single();

        var historyType = hasHistoryInterface.GetGenericArguments()[1]; // IHasHistory<TBase, THistory>
        var historyInstance = Activator.CreateInstance(historyType)!;

        var baseProperties = entry.Properties;

        foreach (var property in baseProperties)
        {
            var prop = historyType.GetProperty(property.Metadata.Name);
            prop!.SetValue(historyInstance, property.CurrentValue);
        }

        switch (entryReference.State)
        {
            case EntityState.Added:
                historyType.GetProperty(HistoryPropertyNames.CreatedAt)!.SetValue(historyInstance, DateTime.Now);
                break;
        }

        return historyInstance;
    }

    private static bool HasChanges(EntityEntryReference entryReference)
    {
        var currentValues = entryReference.CurrentValues;
        var originalValues = entryReference.OriginalValues;

        foreach (var property in currentValues.Properties)
        {
            var currentValue = currentValues[property.Name];
            var originalValue = originalValues[property.Name];

            if (!Equals(currentValue, originalValue))
            {
                return true;
            }
        }

        return false;
    }
}
