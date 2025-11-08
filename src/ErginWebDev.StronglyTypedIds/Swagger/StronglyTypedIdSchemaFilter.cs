using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ErginWebDev.StronglyTypedIds.Swagger;

/// <summary>
/// Schema filter for Swagger/OpenAPI that maps strongly-typed ID wrapper types to their underlying value types.
/// This filter detects value types with a "Value" property and maps them to appropriate OpenAPI schema representations
/// based on the underlying type (Guid, int, long, string, decimal, double, DateTime, DateTimeOffset, or enums).
/// </summary>
/// <remarks>
/// This filter is useful when using wrapper types for IDs (e.g., CustomerId, OrderId) to ensure they are
/// properly represented in the OpenAPI schema as their underlying primitive types rather than as complex objects.
/// The filter supports common primitive types and enum types, automatically generating enum values for enum-based IDs.
/// </remarks>
public sealed class StronglyTypedIdSchemaFilter : ISchemaFilter
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

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsValueType)
            return;

        var valueProperty = context.Type.GetProperty("Value");
        if (valueProperty == null)
            return;

        var valueType = valueProperty.PropertyType;

        if (SupportedValueTypes.Contains(valueType) || valueType.IsEnum)
        {
            MapTypeToSchema(schema, valueType);
        }
    }

    private static void MapTypeToSchema(OpenApiSchema schema, Type valueType)
    {
        switch (valueType)
        {
            case Type t when t == typeof(Guid):
                schema.Type = "string";
                schema.Format = "uuid";
                break;
            case Type t when t == typeof(int):
                schema.Type = "integer";
                schema.Format = "int32";
                break;
            case Type t when t == typeof(long):
                schema.Type = "integer";
                schema.Format = "int64";
                break;
            case Type t when t == typeof(string):
                schema.Type = "string";
                schema.Format = null;
                break;
            case Type t when t == typeof(decimal):
                schema.Type = "number";
                schema.Format = "decimal";
                break;
            case Type t when t == typeof(double):
                schema.Type = "number";
                schema.Format = "double";
                break;
            case Type t when t == typeof(DateTime):
                schema.Type = "string";
                schema.Format = "date-time";
                break;
            case Type t when t == typeof(DateTimeOffset):
                schema.Type = "string";
                schema.Format = "date-time";
                break;
            case Type t when t.IsEnum:
                schema.Type = "string";
                schema.Format = null;
                schema.Enum = new List<Microsoft.OpenApi.Any.IOpenApiAny>();
                foreach (var enumValue in Enum.GetValues(t))
                {
                    schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumValue.ToString()!));
                }
                break;
        }
    }
}
