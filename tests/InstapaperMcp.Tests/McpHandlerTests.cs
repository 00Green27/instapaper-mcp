using System.Text.Json;
using InstapaperMcp.Api.Handlers;
using InstapaperMcp.Application.Interfaces;
using Moq;
using Xunit;

namespace InstapaperMcp.Tests;

public class McpHandlerTests
{
    private readonly Mock<IInstapaperService> _mockService;
    private readonly McpHandler _handler;

    public McpHandlerTests()
    {
        _mockService = new Mock<IInstapaperService>();
        _handler = new McpHandler(_mockService.Object);
    }

    [Fact]
    public async Task Initialize_ReturnsCapabilities()
    {
        var result = await _handler.HandleAsync("initialize", null, CancellationToken.None);

        Assert.NotNull(result);
        var json = JsonSerializer.Serialize(result);
        Assert.Contains("protocolVersion", json);
        Assert.Contains("capabilities", json);
    }

    [Fact]
    public async Task ListTools_ReturnsTools()
    {
        var result = await _handler.HandleAsync("tools/list", null, CancellationToken.None);

        Assert.NotNull(result);
        var json = JsonSerializer.Serialize(result);
        Assert.Contains("search_bookmarks", json);
    }

    [Fact]
    public async Task CallTool_SearchBookmarks_CallsService()
    {
        using var doc = JsonDocument.Parse(
            "{\"name\": \"search_bookmarks\", \"arguments\": {\"query\": \"test\"}}"
        );
        var parameters = doc.RootElement;

        _mockService.Setup(x =>
                x.SearchBookmarksAsync(It.IsAny<long?>(), It.IsAny<string?>(), It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Domain.Common.Result<IReadOnlyList<Domain.Entities.Bookmark>>.Success(
                    new List<Domain.Entities.Bookmark>()));

        await _handler.HandleAsync("tools/call", parameters, CancellationToken.None);

        _mockService.Verify(x => x.SearchBookmarksAsync(
            It.IsAny<long?>(), "test", 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
