namespace Instapaper.Mcp.Server.Tests;

/// <summary>
/// Tests for InstapaperResources.
/// </summary>
public class InstapaperResourcesTests
{
    [Fact]
    public async Task GetUnreadBookmarksAsync_CallsClientWithLimit()
    {
        var client = new TestHelpers.TestClients.SpyClient();
        var resources = new InstapaperResources(client);

        var result = await resources.GetUnreadBookmarksAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.NotNull(client.LastLimit); // Should have a limit set
    }

    [Fact]
    public async Task GetArchivedBookmarksAsync_CallsClientWithArchiveFolder()
    {
        var client = new TestHelpers.TestClients.SpyClient();
        var resources = new InstapaperResources(client);

        var result = await resources.GetArchivedBookmarksAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("archive", client.LastFolderId);
    }

    [Fact]
    public async Task GetStarredBookmarksAsync_CallsClientWithStarredFolder()
    {
        var client = new TestHelpers.TestClients.SpyClient();
        var resources = new InstapaperResources(client);

        var result = await resources.GetStarredBookmarksAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("starred", client.LastFolderId);
    }

    [Fact]
    public async Task GetFoldersAsync_ReturnsFolders()
    {
        var client = new TestHelpers.TestClients.SpyClient();
        var resources = new InstapaperResources(client);

        var result = await resources.GetFoldersAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(client.ListFoldersCalled);
    }

    [Fact]
    public async Task GetBookmarkContentAsync_ReturnsContent()
    {
        var handler = new TestHelpers.TestClients.TestHandler(_ =>
            new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("Test content", System.Text.Encoding.UTF8, "text/plain")
            });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
        var client = TestHelpers.TestClients.CreateClientUnderTest(httpClient);
        var resources = new InstapaperResources(client);

        var result = await resources.GetBookmarkContentAsync(123, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Test content", result);
    }

    [Fact]
    public async Task GetBookmarkContentAsync_WithInvalidId_ThrowsException()
    {
        var client = new TestHelpers.TestClients.SpyClient();
        var resources = new InstapaperResources(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            resources.GetBookmarkContentAsync(0, CancellationToken.None));
    }
}
