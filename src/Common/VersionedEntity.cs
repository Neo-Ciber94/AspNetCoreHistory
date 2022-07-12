namespace AspNetCoreHistory.Common;

public record VersionedEntity<T>(T Value, long Version);