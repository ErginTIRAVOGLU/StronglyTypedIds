using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ErginWebDev.StronglyTypedIds.EFCore;

/// <summary>
/// Generic value converter for strongly typed IDs with any underlying value type.
/// Automatically extracts and stores the Value property in the database.
/// </summary>
/// <typeparam name="TId">The strongly typed ID type (must be a value type with a Value property)</typeparam>
/// <typeparam name="TValue">The underlying value type (Guid, int, long, string, etc.)</typeparam>
public sealed class StronglyTypedIdValueConverter<TId, TValue> : ValueConverter<TId, TValue>
    where TId : struct
{
    public StronglyTypedIdValueConverter(ConverterMappingHints? mappingHints = null)
        : base(
            id => (TValue)id.GetType().GetProperty("Value")!.GetValue(id)!,
            value => (TId)Activator.CreateInstance(typeof(TId), value)!,
            mappingHints
        )
    { }
}
