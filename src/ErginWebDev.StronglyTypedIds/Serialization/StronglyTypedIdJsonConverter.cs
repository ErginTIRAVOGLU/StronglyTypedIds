using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErginWebDev.StronglyTypedIds.Serialization;

/// <summary>
/// A custom JSON converter for strongly-typed ID structs that wraps primitive values.
/// Serializes and deserializes strongly-typed IDs by reading/writing their underlying Value property.
/// </summary>
/// <typeparam name="T">The strongly-typed ID struct type that contains a Value property.</typeparam>
/// <remarks>
/// This converter supports the following underlying value types:
/// <list type="bullet">
/// <item><description><see cref="Guid"/></description></item>
/// <item><description><see cref="int"/></description></item>
/// <item><description><see cref="long"/></description></item>
/// <item><description><see cref="string"/></description></item>
/// <item><description><see cref="decimal"/></description></item>
/// <item><description><see cref="double"/></description></item>
/// <item><description><see cref="DateTime"/></description></item>
/// <item><description><see cref="DateTimeOffset"/></description></item>
/// <item><description>Enum types</description></item>
/// </list>
/// The strongly-typed ID must be a struct with a public Value property and a constructor that accepts the underlying value type.
/// </remarks>
/// <exception cref="JsonException">
/// Thrown when the type does not have a Value property or when the underlying value type is not supported.
/// </exception>
public class StronglyTypedIdJsonConverter<T> : JsonConverter<T> where T : struct
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

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valueProperty = typeToConvert.GetProperty("Value");
        if (valueProperty == null)
            throw new JsonException($"Type {typeToConvert.Name} does not have a Value property");

        var valueType = valueProperty.PropertyType;
        object value = ReadValue(ref reader, valueType);
        
        return (T)Activator.CreateInstance(typeof(T), value)!;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var valueProperty = value.GetType().GetProperty("Value");
        if (valueProperty == null)
            throw new JsonException($"Type {value.GetType().Name} does not have a Value property");

        var underlyingValue = valueProperty.GetValue(value)!;
        WriteValue(writer, underlyingValue, options);
    }

    private static object ReadValue(ref Utf8JsonReader reader, Type valueType)
    {
        return valueType switch
        {
            Type t when t == typeof(Guid) => reader.GetGuid(),
            Type t when t == typeof(int) => reader.GetInt32(),
            Type t when t == typeof(long) => reader.GetInt64(),
            Type t when t == typeof(string) => reader.GetString() ?? string.Empty,
            Type t when t == typeof(decimal) => reader.GetDecimal(),
            Type t when t == typeof(double) => reader.GetDouble(),
            Type t when t == typeof(DateTime) => reader.GetDateTime(),
            Type t when t == typeof(DateTimeOffset) => reader.GetDateTimeOffset(),
            Type t when t.IsEnum => Enum.Parse(t, reader.GetString()!),
            _ => throw new JsonException($"Unsupported value type: {valueType.Name}")
        };
    }

    private static void WriteValue(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case Guid guidValue:
                writer.WriteStringValue(guidValue);
                break;
            case int intValue:
                writer.WriteNumberValue(intValue);
                break;
            case long longValue:
                writer.WriteNumberValue(longValue);
                break;
            case string stringValue:
                writer.WriteStringValue(stringValue);
                break;
            case decimal decimalValue:
                writer.WriteNumberValue(decimalValue);
                break;
            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                break;
            case DateTime dateTimeValue:
                writer.WriteStringValue(dateTimeValue);
                break;
            case DateTimeOffset dateTimeOffsetValue:
                writer.WriteStringValue(dateTimeOffsetValue);
                break;
            case Enum enumValue:
                writer.WriteStringValue(enumValue.ToString());
                break;
            default:
                throw new JsonException($"Unsupported value type: {value.GetType().Name}");
        }
    }
}
