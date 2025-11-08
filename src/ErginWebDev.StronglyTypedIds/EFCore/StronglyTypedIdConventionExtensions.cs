using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace ErginWebDev.StronglyTypedIds.EFCore;

/// <summary>
/// Provides extension methods for configuring strongly typed ID conventions in Entity Framework Core.
/// </summary>
public static class StronglyTypedIdConventionExtensions
{
    public static ModelConfigurationBuilder ConfigureStronglyTypedIds(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new StronglyTypedIdConvention());
        return configurationBuilder;
    }
}

internal sealed class StronglyTypedIdConvention : IModelFinalizingConvention
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

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var clrType = property.ClrType;
                if (!clrType.IsValueType)
                    continue;

                var valueProperty = clrType.GetProperty("Value");
                if (valueProperty == null)
                    continue;

                var valueType = valueProperty.PropertyType;
                
                // Support direct types and enums
                if (SupportedValueTypes.Contains(valueType) || valueType.IsEnum)
                {
                    var converterType = typeof(StronglyTypedIdValueConverter<,>).MakeGenericType(clrType, valueType);
                    property.SetValueConverter(converterType);
                }
            }
        }
    }
}
