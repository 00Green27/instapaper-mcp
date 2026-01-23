using System.Net;
using System.Text;
using System.Text.Json;
using InstapaperMcp.Domain.Entities;
using InstapaperMcp.Infrastructure.Configuration;
using InstapaperMcp.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace InstapaperMcp.Tests;

public class InstapaperServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpHandler;
    private readonly InstapaperService _service;

    public InstapaperServiceTests()
    {
        _httpHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpHandler.Object)
        {
            BaseAddress = new Uri("https://www.instapaper.com/api/1/")
        };

        var options = Options.Create(new InstapaperOptions
        {
            ConsumerKey = "key",
            ConsumerSecret = "secret",
            AccessToken = "token",
            AccessTokenSecret = "tokenSecret"
        });

        _service = new InstapaperService(httpClient, options, Mock.Of<ILogger<InstapaperService>>());
    }

    [Fact]
    public async Task SearchBookmarksAsync_FiltersByQuery()
    {
        // Arrange
        var bookmarks = new[]
        {
            new
            {
                type = "bookmark", bookmark_id = 1, url = "http://example.com/1", title = "Relevant Title",
                description = "", time = 1234567890, starred = "0"
            },
            new
            {
                type = "bookmark", bookmark_id = 2, url = "http://example.com/2", title = "Unrelated",
                description = "", time = 1234567890, starred = "0"
            },
            new
            {
                type = "bookmark", bookmark_id = 3, url = "http://example.com/3", title = "Another Relevant",
                description = "Contains relevant info", time = 1234567890, starred = "0"
            }
        };

        var jsonResponse = JsonSerializer.Serialize(bookmarks);

        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.SearchBookmarksAsync(null, "Relevant", 10, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, b => b.Id == 1);
        Assert.Contains(result.Value, b => b.Id == 3);
        Assert.DoesNotContain(result.Value, b => b.Id == 2);
    }
}
