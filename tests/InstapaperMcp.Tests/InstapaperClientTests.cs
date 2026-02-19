using System.Net;
using System.Text;

using Instapaper.Mcp.Server.Tests.TestHelpers;

namespace Instapaper.Mcp.Server.Tests;

public class InstapaperClientTests
{
    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new TestClients.TestHandler(responder);
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.instapaper.test/")
        };
        return client;
    }

    private static InstapaperClient CreateClientUnderTest(HttpClient httpClient)
    {
        return TestClients.CreateClientUnderTest(httpClient);
    }

    [Fact]
    public async Task ListBookmarksAsync_ReturnsBookmarks_WhenApiReturnsBookmarks()
    {
        var json = "[ { \"type\": \"bookmark\", \"bookmark_id\": 1, \"url\": \"http://a\", \"title\": \"A\", \"folder_id\": 0 }, { \"type\": \"bookmark\", \"bookmark_id\": 2, \"url\": \"http://b\", \"title\": \"B\", \"folder_id\": 0 } ]";

        var client = CreateClient(req =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var sut = CreateClientUnderTest(client);

        var results = await sut.ListBookmarksAsync(query: null, folderId: null, limit: 2);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, b => b.Title == "A");
        Assert.Contains(results, b => b.Title == "B");
    }

    [Fact]
    public async Task AddBookmarkAsync_ReturnsBookmark_WhenApiReturnsBookmark()
    {
        var json = "[ { \"type\": \"bookmark\", \"bookmark_id\": 10, \"url\": \"http://x\", \"title\": \"X\", \"folder_id\": 1 } ]";

        var client = CreateClient(req => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var sut = CreateClientUnderTest(client);

        var bookmark = await sut.AddBookmarkAsync(url: "http://x", title: "X");

        Assert.Equal(10, bookmark.BookmarkId);
        Assert.Equal("X", bookmark.Title);
    }
}