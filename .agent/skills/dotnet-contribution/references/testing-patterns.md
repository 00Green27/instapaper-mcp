# Testing Patterns in .NET

## Unit Testing (xUnit + Moq)

Focus on testing business logic in isolation.

```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepo = new();
    private readonly OrderService _sut;

    public OrderServiceTests() {
        _sut = new OrderService(_mockRepo.Object);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_Success()
    {
        // Arrange
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateOrderAsync(new CreateOrderRequest());

        // Assert
        Assert.True(result.IsSuccess);
        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    }
}
```

## Integration Testing (WebApplicationFactory)

Test the entire stack, including middleware and routing, but mock external dependencies if needed.

```csharp
public class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetProduct_ReturnsSuccess()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/products/1");
        
        response.EnsureSuccessStatusCode();
    }
}
```

## Best Practices
- **Mock Interfaces**: Only mock what you don't own or what involves external IO.
- **FluentAssertions**: Use for more readable assertions.
- **Test Containers**: Use Docker for database integration tests (recommended for CI/CD).
