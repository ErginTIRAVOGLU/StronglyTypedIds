using ErginWebDev.StronglyTypedIds.Swagger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.Json;
using ErginWebDev.StronglyTypedIds.Serialization;

namespace ErginWebDev.StronglyTypedIds;


/// <summary>
/// Provides extension methods for configuring strongly typed IDs in the service collection.
/// Supports Swagger, Scalar API, and other OpenAPI-compatible documentation tools.
/// </summary>
public static class StronglyTypedIdsSetupExtensions
{
    /// <summary>
    /// Adds strongly typed ID support for JSON serialization and Swagger/OpenAPI documentation.
    /// The OpenAPI schema filter works with Swagger UI, Scalar, and other OpenAPI-compatible tools.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureSwagger">Whether to configure Swagger (default: true). 
    /// Set to false if you're using Scalar or have already configured Swagger separately.</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStronglyTypedIds(this IServiceCollection services, bool configureSwagger = true)
    {
        // JSON Serialization - Required for all API scenarios
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
        });

        // OpenAPI Schema Filter - Works with Swagger, Scalar, and other OpenAPI tools
        if (configureSwagger)
        {
            services.AddSwaggerGen(options =>
            {
                options.SchemaFilter<StronglyTypedIdSchemaFilter>();
            });
        }

        return services;
    }
    
    /// <summary>
    /// Adds only JSON serialization support for strongly typed IDs.
    /// Use this if you don't need OpenAPI/Swagger documentation.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStronglyTypedIdsJsonOnly(this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new StronglyTypedIdJsonConverterFactory());
        });

        return services;
    }
}
