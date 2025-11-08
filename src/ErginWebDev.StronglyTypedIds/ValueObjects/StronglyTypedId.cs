namespace ErginWebDev.StronglyTypedIds.ValueObjects;

/// <summary>
/// Base record struct for strongly typed IDs with generic value type support.
/// Supported types: Guid, int, long, string, decimal, double, DateTime, DateTimeOffset, Enum
/// </summary>
/// <typeparam name="TValue">The underlying value type for the ID</typeparam>
public readonly record struct StronglyTypedId<TValue>(TValue Value)
{
    public override string ToString() => Value?.ToString() ?? string.Empty;
}

/// <summary>
/// Specialized Guid-based strongly typed ID with factory methods.
/// </summary>
public readonly record struct StronglyTypedId(Guid Value) : IEquatable<StronglyTypedId>
{
#if NET9_0_OR_GREATER
    /// <summary>
    /// Creates a new ID using Guid.CreateVersion7() (time-ordered, recommended for database performance).
    /// Available in .NET 9.0 and later.
    /// </summary>
    public static StronglyTypedId NewId() => new(Guid.CreateVersion7());
#else
    /// <summary>
    /// Creates a new ID using Guid.NewGuid() (random).
    /// Note: Guid.CreateVersion7() is only available in .NET 9.0+. This method uses Guid.NewGuid() for .NET 8.0.
    /// </summary>
    public static StronglyTypedId NewId() => new(Guid.NewGuid());
#endif
    
    /// <summary>
    /// Creates a new ID using Guid.NewGuid() (random, traditional approach)
    /// </summary>
    public static StronglyTypedId NewGuid() => new(Guid.NewGuid());
    
    /// <summary>
    /// Empty Guid value
    /// </summary>
    public static StronglyTypedId Empty => new(Guid.Empty);
    
    public override string ToString() => Value.ToString();
}
