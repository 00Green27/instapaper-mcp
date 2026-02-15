namespace Instapaper.Mcp.Server.Tests;

public class InstapaperFolderToolsTests
{
    [Fact]
    public async Task ListFoldersAsync_InvokesClient()
    {
        var spy = new TestHelpers.TestClients.SpyClient();
        var tools = new InstapaperFolderTools(spy);

        var folders = await tools.ListFolderAsync(CancellationToken.None);

        Assert.Single(folders);
        Assert.True(spy.ListFoldersCalled);
        Assert.Equal("Inbox", folders.First().Title);
    }
}
