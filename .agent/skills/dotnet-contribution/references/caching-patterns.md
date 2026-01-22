# Caching Patterns in .NET

## Multi-Level Caching (L1/L2)

Combine `IMemoryCache` (fast, in-process) with `IDistributedCache` (Redis, shared).

```csharp
public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
{
    var cacheKey = $"product:{id}";

    // L1: Memory cache
    if (_memoryCache.TryGetValue(cacheKey, out Product? cached))
        return cached;

    // L2: Distributed cache (Redis)
    var distributed = await _distributedCache.GetStringAsync(cacheKey, ct);
    if (distributed != null)
    {
        var product = JsonSerializer.Deserialize<Product>(distributed);
        _memoryCache.Set(cacheKey, product, TimeSpan.FromMinutes(1)); // Populate L1
        return product;
    }

    // L3: Database
    var fromDb = await _repository.GetByIdAsync(id, ct);
    if (fromDb != null)
    {
        await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(fromDb), ct);
        _memoryCache.Set(cacheKey, fromDb, TimeSpan.FromMinutes(1));
    }

    return fromDb;
}
```

## Stale-While-Revalidate

Return stale data immediately while updating the cache in the background.

```csharp
public async Task<T?> GetOrCreateAsync(string key, Func<CancellationToken, Task<T>> factory)
{
    var cached = await _cache.GetAsync(key);
    if (cached != null)
    {
        if (cached.IsStale) 
        {
             _ = Task.Run(async () => await RefreshCache(key, factory));
        }
        return cached.Value;
    }
    // ... materialization logic
}
```
