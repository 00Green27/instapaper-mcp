namespace Instapaper.Mcp.Server.Tests;

public class InstapaperBookmarkToolsTests
{
    [Fact]
    public async Task ListBookmarksAsync_ParsesLimitAndForwardsToClient()
    {
        var spy = new TestHelpers.TestClients.SpyClient();
        var tools = new InstapaperBookmarkTools(spy);

        var results = await tools.ListBookmarksAsync(query: "test", folderId: "unread", limit: "5", CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("test", spy.LastQuery);
        Assert.Equal("unread", spy.LastFolderId);
        Assert.Equal(5, spy.LastLimit);
    }
}