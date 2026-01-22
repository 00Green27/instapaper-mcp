# Dependency Injection Patterns in .NET

## Service Registration

Use the `IServiceCollection` extensions to organize your registrations.

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Scoped: One instance per HTTP request (Services, Repositories)
        services.AddScoped<IProductService, ProductService>();
        
        // Singleton: One instance for app lifetime (Cache, Config, Clients)
        services.AddSingleton<ICacheService, RedisCacheService>();

        // Transient: New instance every time (Validators, lightweight objects)
        services.AddTransient<IValidator<CreateOrderRequest>, CreateOrderValidator>();

        // Keyed services (.NET 8+)
        services.AddKeyedScoped<IPaymentProcessor, StripeProcessor>("stripe");
        services.AddKeyedScoped<IPaymentProcessor, PayPalProcessor>("paypal");

        return services;
    }
}
```

## Configuration with IOptions

Always use typed configuration instead of reading directly from `IConfiguration`.

```csharp
// Definition
public class RedisOptions {
    public const string SectionName = "Redis";
    public string Connection { get; set; } = "localhost:6379";
}

// Registration
services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

// Usage
public class MyService(IOptions<RedisOptions> options) {
    private readonly RedisOptions _options = options.Value;
}
```

### IOptions Variations
- `IOptions<T>`: Singleton, read once.
- `IOptionsSnapshot<T>`: Scoped, re-reads per request (useful for dynamic config).
- `IOptionsMonitor<T>`: Singleton, updates when file changes.
