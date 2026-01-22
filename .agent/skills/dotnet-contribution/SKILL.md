---
name: dotnet-contribution
description: Master C#/.NET development for building robust APIs, MCP servers, and enterprise applications. Covers architecture, async/await, DI, EF Core, Dapper, and testing. Use when designing, implementing, or reviewing modern .NET backends.
---

# .NET Backend Development

Expert guidance for building production-grade APIs and enterprise backends with modern C# (12+) and .NET 8+.

## Architectural Philosophy

Follow **Clean Architecture** and **SOLID** principles. Focus on:
- **Separation of Concerns**: Domain (logic) vs. Infrastructure (external) vs. API (entry).
- **Testability**: Everything must be injectable and mockable.
- **Performance**: Async all the way, efficient data access, and aggressive caching.

## Core Patterns

### 1. Dependency Injection (DI)
Use constructor injection for all dependencies. Prefer **Scoped** for services, **Singleton** for stateless utilities/cache, and **Keyed Services** (.NET 8+) for multiple implementations.

- **Options Pattern**: Map configuration to classes using `IOptions<T>`.
- **Details**: See `references/dependency-injection.md` (TODO)

### 2. Async/Await
Async all the way down. Never block on async code (`.Result` or `.Wait()`).
- Use `CancellationToken` in all async methods.
- Use `ValueTask` for hot paths where caching is common.
- **Details**: See [Async Patterns](references/async-patterns.md) (TODO)

### 3. Result Pattern
Avoid using exceptions for business flow control. Use a `Result<T>` type to return success or failure explicitly.
```csharp
public async Task<Result<Order>> CreateOrderAsync(CreateOrderRequest request) {
    if (!await _stock.IsAvailable(request.ProductId))
        return Result.Failure("No stock");
    // ...
}
```

## Specialized Reference Guides

- **Entity Framework Core**: [EF Core Best Practices](references/ef-core-best-practices.md)
- **Dapper**: [Dapper Patterns](references/dapper-patterns.md)
- **Testing**: [Testing Patterns](references/testing-patterns.md) (TODO)
- **Caching**: [Caching Patterns](references/caching-patterns.md) (TODO)

## Quality Standards (DO/DON'T)

### DO
1. **Use CancellationToken** everywhere.
2. **Inject IHttpClientFactory** instead of creating `new HttpClient()`.
3. **Use record types** for DTOs and immutable data structures.
4. **Follow naming conventions**: `IInterface`, `_privateField`, `MethodAsync`.
5. **Implement Health Checks** for all dependencies.

### DON'T
1. **Don't expose EF entities** directly in APIs; use DTOs/ViewModels.
2. **Don't use `async void`** (except for event handlers).
3. **Don't hardcode configuration**; use `appsettings.json` and `IOptions`.
4. **Don't catch generic `Exception`** without logging or re-throwing.

## Project Structure (Clean Architecture)

```
src/
├── Domain/         # Entity, Interfaces, Exceptions (No dependencies)
├── Application/    # Services, DTOs, Handlers
├── Infrastructure/ # EF Core, Dapper, External APIs, Redis
└── Api/            # Controllers, Middleware, Program.cs
```

## Templates & Assets
- [Repository Template](assets/repository-template.cs)
- [Service Template](assets/service-template.cs)
