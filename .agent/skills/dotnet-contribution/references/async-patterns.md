# Async/Await Patterns in .NET

## Best Practices

### ✅ CORRECT: Async all the way down
```csharp
public async Task<Product> GetProductAsync(string id, CancellationToken ct = default)
{
    return await _repository.GetByIdAsync(id, ct);
}
```

### ✅ CORRECT: Parallel execution with WhenAll
```csharp
public async Task<(Stock, Price)> GetStockAndPriceAsync(string productId, CancellationToken ct = default)
{
    var stockTask = _stockService.GetAsync(productId, ct);
    var priceTask = _priceService.GetAsync(productId, ct);

    await Task.WhenAll(stockTask, priceTask);
    return (await stockTask, await priceTask);
}
```

### ✅ CORRECT: ConfigureAwait in libraries
Use `.ConfigureAwait(false)` in library code (Infrastructure/Domain) to avoid synchronization context overhead.
```csharp
var result = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
```

### ✅ CORRECT: ValueTask for hot paths
Use `ValueTask` when the result is often available synchronously (e.g., from cache).
```csharp
public ValueTask<Product?> GetCachedProductAsync(string id)
{
    if (_cache.TryGetValue(id, out Product? product))
        return ValueTask.FromResult(product);

    return new ValueTask<Product?>(GetFromDatabaseAsync(id));
}
```

## Anti-Patterns

### ❌ WRONG: Blocking on async
```csharp
var result = GetProductAsync(id).Result;  // Deadlock risk!
var result2 = GetProductAsync(id).GetAwaiter().GetResult(); // Still bad.
```

### ❌ WRONG: async void
Exceptions in `async void` methods cannot be caught and will crash the process. Use `async Task` instead, except for UI event handlers.

### ❌ WRONG: Unnecessary Task.Run
Don't wrap already asynchronous code in `Task.Run()`. It just wastes a thread pool thread.
