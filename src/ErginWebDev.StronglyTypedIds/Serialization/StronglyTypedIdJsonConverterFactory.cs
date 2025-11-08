using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErginWebDev.StronglyTypedIds.Serialization;

/// <summary>
/// A JSON converter factory that creates converters for strongly-typed ID value types.
/// Supports value types with a "Value" property of supported primitive types (Guid, int, long, string, decimal, double, DateTime, DateTimeOffset) or enums.
/// </summary>
/// <remarks>
/// This factory automatically handles serialization and deserialization of strongly-typed ID patterns
/// by delegating to <see cref="StronglyTypedIdJsonConverter{T}"/> for compatible types.
/// The type to convert must be a value type (struct) and contain a public "Value" property.
/// </remarks>
public sealed class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
{
    private static readonly HashSet<Type> SupportedValueTypes = new()
    {
        typeof(Guid),
        typeof(int),
        typeof(long),
        typeof(string),
        typeof(decimal),
        typeof(double),
        typeof(DateTime),
        typeof(DateTimeOffset)
    };

    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsValueType)
            return false;

        var valueProperty = typeToConvert.GetProperty("Value");
        if (valueProperty == null)
            return false;

        var valueType = valueProperty.PropertyType;
        return SupportedValueTypes.Contains(valueType) || valueType.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(StronglyTypedIdJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}
