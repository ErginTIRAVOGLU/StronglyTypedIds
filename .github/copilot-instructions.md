# StronglyTypedIds Library - AI Coding Instructions

## Project Overview
This is a DDD-compliant NuGet package providing strongly typed ID infrastructure using `record struct` with automatic integration for EF Core, System.Text.Json, and Swagger/OpenAPI.

**Core Pattern**: All strongly typed IDs inherit from `StronglyTypedId<TValue>(TValue Value)` record struct supporting multiple value types: **Guid, int, long, string, decimal, double, DateTime, DateTimeOffset, and Enum**.

## Architecture & Key Components

### 1. Base Value Object (`ValueObjects/StronglyTypedId.cs`)
- **Generic Pattern**: `readonly record struct StronglyTypedId<TValue>(TValue Value)` 
- **Supported Types**: Guid, int, long, string, decimal, double, DateTime, DateTimeOffset, Enum
- **Guid-specific factory methods**:
  - `NewId()` - Uses `Guid.CreateVersion7()` on .NET 9.0+ (time-ordered, recommended for database performance)
  - `NewId()` - Falls back to `Guid.NewGuid()` on .NET 8.0 (CreateVersion7 not available)
  - `NewGuid()` - Uses `Guid.NewGuid()` (random, traditional approach, works on all versions)
  - Both methods are available, choose based on your needs
- **Usage Examples**:
  - `public readonly record struct CustomerId(Guid Value) : StronglyTypedId<Guid>(Value);`
  - `public readonly record struct OrderNumber(int Value) : StronglyTypedId<int>(Value);`
  - `public readonly record struct ProductCode(string Value) : StronglyTypedId<string>(Value);`

### 2. Reflection-Based Auto-Discovery
All converters/filters use reflection to detect strongly typed IDs:
```csharp
// Detection pattern used throughout:
var valueProperty = type.GetProperty("Value");
if (valueProperty != null && type.IsValueType)
{
    var valueType = valueProperty.PropertyType;
    // Check if valueType is in supported types or is Enum
    if (SupportedValueTypes.Contains(valueType) || valueType.IsEnum)
    {
        // Apply converter for TId and TValue
    }
}
```

### 3. EF Core Integration (`EFCore/`)
- **Convention-based**: `ConfigureStronglyTypedIds()` extension on `ModelConfigurationBuilder`
- **Automatic conversion**: Detects all value types with a `Value` property of supported types
- **Storage**: Converts to underlying value type in database using `StronglyTypedIdValueConverter<TId, TValue>`
- **Supported storage types**: Guid, int, long, string, decimal, double, DateTime, DateTimeOffset, Enum
- **Usage in DbContext**:
```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    configurationBuilder.ConfigureStronglyTypedIds();
}
```

### 4. JSON Serialization (`Serialization/`)
- **Factory pattern**: `StronglyTypedIdJsonConverterFactory` auto-registers converters
- **Type-aware serialization**: 
  - Guid → string: `"550e8400-e29b-41d4-a716-446655440000"`
  - int/long → number: `123`, `9999999999`
  - string → string: `"PROD-001"`
  - decimal/double → number: `99.95`, `123.456`
  - DateTime/DateTimeOffset → ISO 8601 string: `"2025-11-08T10:30:00Z"`
  - Enum → string: `"Active"`
- **Deserialization**: Reads appropriate JSON type, constructs value type via Activator

### 5. Swagger/OpenAPI (`Swagger/`)
- **Schema filter**: `StronglyTypedIdSchemaFilter` transforms schemas based on underlying value type
- **Type mapping**:
  - Guid → `type: "string", format: "uuid"`
  - int → `type: "integer", format: "int32"`
  - long → `type: "integer", format: "int64"`
  - string → `type: "string"`
  - decimal/double → `type: "number"`
  - DateTime/DateTimeOffset → `type: "string", format: "date-time"`
  - Enum → `type: "string"` with enum values
- **Result**: APIs document IDs with appropriate OpenAPI types, not complex objects

### 6. Setup Extension (`StronglyTypedIdsSetupExtensions.cs`)
- **Single registration point**: `services.AddStronglyTypedIds()`
- **Configures**: JSON serialization (via JsonOptions) + OpenAPI schema generation
- **Swagger & Scalar Compatible**: Schema filter works with Swagger UI, Scalar API, and other OpenAPI tools
- **Flexible setup options**:
  - `AddStronglyTypedIds()` - Full setup with Swagger
  - `AddStronglyTypedIds(configureSwagger: false)` - Skip Swagger config (for Scalar-only)
  - `AddStronglyTypedIdsJsonOnly()` - JSON only, no OpenAPI
- **Note**: EF Core setup is separate (use `ConfigureConventions` in DbContext)

## Critical Conventions

### When Adding New Features
1. **Maintain reflection-based detection**: All integrations rely on the pattern `IsValueType + Guid Value property`
2. **No manual registration**: Keep the auto-discovery approach (convention over configuration)
3. **Generic converters**: Use `Activator.CreateInstance` with generic type parameters for type-agnostic handling

### Code Style
- **File-scoped namespaces**: All files use `namespace X;` (not blocks)
- **Sealed classes**: Converters/filters are `sealed` to prevent inheritance
- **Null safety**: Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- **Minimal APIs**: Prefer extension methods over services/middleware

## Project Structure
```
src/ErginWebDev.StronglyTypedIds/
  ├── ValueObjects/          # Base StronglyTypedId record struct
  ├── EFCore/                # Database persistence (convention + converter)
  ├── Serialization/         # JSON converters (factory + generic converter)
  ├── Swagger/               # OpenAPI schema customization
  └── StronglyTypedIdsSetupExtensions.cs  # DI registration
```

## Build & Package
- **Target**: Multi-targeting - .NET 8.0 and .NET 9.0 (`net8.0;net9.0`)
- **NuGet ID**: `ErginWebDev.StronglyTypedIds`
- **Auto-package**: `<GeneratePackageOnBuild>true</GeneratePackageOnBuild>`
- **Dependencies**: Microsoft.EntityFrameworkCore 8.0+, Swashbuckle.AspNetCore 6.8+
- **Build**: `dotnet build` in `src/` directory generates NuGet package for both targets
- **Pack manually**: `dotnet pack -c Release` for production builds
- **Note**: Guid.CreateVersion7() is only available in .NET 9.0+ (conditional compilation used)

## Usage Examples

### Creating Custom Strongly Typed IDs
```csharp
// Domain entities define their own ID types with various value types
public readonly record struct CustomerId(Guid Value) : StronglyTypedId<Guid>(Value);
public readonly record struct OrderNumber(int Value) : StronglyTypedId<int>(Value);
public readonly record struct ProductCode(string Value) : StronglyTypedId<string>(Value);
public readonly record struct InvoiceId(long Value) : StronglyTypedId<long>(Value);
public readonly record struct Price(decimal Value) : StronglyTypedId<decimal>(Value);
public readonly record struct StatusType(OrderStatus Value) : StronglyTypedId<OrderStatus>(Value);

// Enum definition for strongly typed enum IDs
public enum OrderStatus { Pending, Active, Completed, Cancelled }

// Create new IDs - Choose your preferred Guid generation strategy
var customerId1 = new CustomerId(Guid.CreateVersion7()); // Time-ordered (.NET 9+, recommended)
var customerId2 = new CustomerId(Guid.NewGuid());        // Random (works on .NET 8+)

// Other value types
var orderNumber = new OrderNumber(12345);                 // int-based
var productCode = new ProductCode("PROD-001");            // string-based
var invoiceId = new InvoiceId(9999999999L);              // long-based
var price = new Price(99.95m);                            // decimal-based
var status = new StatusType(OrderStatus.Active);          // enum-based

// Use in entity classes
public class Customer
{
    public CustomerId Id { get; init; }
    public string Name { get; set; } = string.Empty;
}

public class Order
{
    public CustomerId CustomerId { get; init; }
    public OrderNumber OrderNumber { get; init; }
    public Price TotalPrice { get; init; }
    public StatusType Status { get; init; }
}
```

### Complete Application Setup

#### Option 1: With Swagger
```csharp
// Program.cs
builder.Services.AddStronglyTypedIds(); // JSON + Swagger

// Standard Swagger setup
app.UseSwagger();
app.UseSwaggerUI();
```

#### Option 2: With Scalar API
```csharp
// Program.cs
builder.Services.AddStronglyTypedIds(configureSwagger: false); // JSON only

// Add Scalar (requires: dotnet add package Scalar.AspNetCore)
builder.Services.AddOpenApi(); // .NET 9+ built-in OpenAPI
app.MapOpenApi();
app.MapScalarApiReference(); // Scalar UI
```

#### Option 3: With Swagger + Scalar Together
```csharp
// Program.cs
builder.Services.AddStronglyTypedIds(); // JSON + Swagger

// Both UIs work with the same OpenAPI schema
app.UseSwagger();
app.UseSwaggerUI();
app.MapScalarApiReference(); // Add Scalar too
```

#### EF Core Configuration (Required for all options)
```csharp
// DbContext
public class AppDbContext : DbContext
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureStronglyTypedIds(); // EF Core
    }
}
```

#### Controller Example
```csharp
// Controller - IDs are automatically serialized/deserialized
[ApiController]
public class CustomersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<Customer> Get(CustomerId id) // Binds from "550e8400-..."
    {
        return await _context.Customers.FindAsync(id);
    }
}
```

### JSON Behavior
```json
// Request/Response format - IDs appear as their underlying value types
{
    "id": "550e8400-e29b-41d4-a716-446655440000",      // Guid → string
    "orderNumber": 12345,                               // int → number
    "productCode": "PROD-001",                          // string → string
    "invoiceId": 9999999999,                            // long → number
    "totalPrice": 99.95,                                // decimal → number
    "status": "Active",                                 // enum → string
    "createdAt": "2025-11-08T10:30:00Z",               // DateTime → ISO string
    "name": "John Doe"
}
```

## Migration Guide

### From Guid-based Entities
```csharp
// Before: Using raw Guids
public class Customer
{
    public Guid Id { get; set; }
}

// After: Strongly typed IDs
public readonly record struct CustomerId(Guid Value) : StronglyTypedId<Guid>(Value);

public class Customer
{
    public CustomerId Id { get; init; } // Changed type only
}
```

**Migration Steps:**
1. Create new strongly typed ID record structs for each entity
2. Update entity properties from `Guid` to custom ID type
3. Add `ConfigureConventions()` in DbContext (EF auto-converts to/from Guid)
4. No database migration needed - stored as Guid (same schema)
5. Update service/repository method signatures to use typed IDs
6. Add `AddStronglyTypedIds()` in DI setup for API projects

## Performance Considerations

### Reflection Overhead
- **Converter construction**: Reflection occurs once during app startup when converters are registered
- **Runtime serialization**: Uses compiled delegates - no per-request reflection
- **EF Core queries**: Converted to Guid at SQL generation time - no performance impact

### When Reflection Executes
```csharp
// Startup (one-time cost)
services.AddStronglyTypedIds(); // Registers factory
configurationBuilder.ConfigureStronglyTypedIds(); // Scans model

// Runtime (zero reflection - uses cached converters)
var json = JsonSerializer.Serialize(customer); // Fast
var entity = await context.Customers.FindAsync(id); // Fast
```

**Best Practice**: The reflection-based discovery is a design-time convenience. If profiling shows startup time issues with hundreds of ID types, consider caching the reflection results.

## Testing Strategy
When adding tests, focus on:

### 1. Unit Tests (Converters)
```csharp
[Fact]
public void JsonConverter_RoundTrip_PreservesValue()
{
    var original = new CustomerId(Guid.CreateVersion7());
    var json = JsonSerializer.Serialize(original);
    var deserialized = JsonSerializer.Deserialize<CustomerId>(json);
    Assert.Equal(original, deserialized);
}

[Fact]
public void EFCoreConverter_DetectsStronglyTypedIds()
{
    var builder = new ModelConfigurationBuilder(new ConventionSet());
    builder.ConfigureStronglyTypedIds();
    // Verify CustomerId properties are configured with converter
}
```

### 2. Integration Tests (EF Core)
```csharp
[Fact]
public async Task DbContext_QueryByStronglyTypedId_Works()
{
    var customerId = new CustomerId(Guid.CreateVersion7());
    var customer = new Customer { Id = customerId, Name = "Test" };
    
    await context.Customers.AddAsync(customer);
    await context.SaveChangesAsync();
    
    var found = await context.Customers.FindAsync(customerId);
    Assert.NotNull(found);
    Assert.Equal(customerId, found.Id);
}
```

### 3. API Tests (Swagger/JSON)
```csharp
[Fact]
public async Task API_AcceptsAndReturnsUuidStrings()
{
    var response = await client.GetAsync("/api/customers/550e8400-e29b-41d4-a716-446655440000");
    var json = await response.Content.ReadAsStringAsync();
    
    // Verify ID serialized as string, not object
    Assert.Contains("\"id\": \"550e8400-", json);
}
```

### 4. Edge Cases
- Empty Guids: `new CustomerId(Guid.Empty)`
- Null JSON values (should throw or use nullable ID types)
- Invalid UUID strings in API requests
- Multiple inheritance levels (if extending custom IDs)

## Common Pitfalls to Avoid
- **Choose wisely**: `Guid.CreateVersion7()` (time-ordered, better for DB indexes) vs `Guid.NewGuid()` (random) - both work, pick based on your requirements
- **Don't** create manual converters per ID type - leverage the generic reflection approach
- **Don't** mix ID types - each entity should have its own strongly typed ID (e.g., `CustomerId`, `OrderId`)
- **Don't** forget both setups: `AddStronglyTypedIds()` for JSON/Swagger + `ConfigureConventions()` for EF Core
- **Don't** forget to specify the generic type parameter when inheriting from `StronglyTypedId<TValue>`
