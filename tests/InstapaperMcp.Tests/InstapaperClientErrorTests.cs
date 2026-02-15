using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Instapaper.Mcp.Server.Tests.TestHelpers;

namespace Instapaper.Mcp.Server.Tests;

public class InstapaperClientErrorTests
{
    private static InstapaperClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var http = new HttpClient(new TestClients.TestHandler(responder)) { BaseAddress = new Uri("https://api.instapaper.test/") };
        var options = Microsoft.Extensions.Options.Options.Create(new Configuration.InstapaperOptions
        {
            ConsumerKey = "ck",
            ConsumerSecret = "cs",
            Username = "u",
            Password = "p"
        });
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<InstapaperClient>();
        return new InstapaperClient(http, new OAuth1SignatureGenerator(TimeProvider.System), options, logger);
    }

    [Fact]
    public async Task SendAsync_ThrowsInstapaperApiException_OnErrorResponse()
    {
        var errorJson = "{ \"type\": \"error\", \"error_code\": 1, \"message\": \"Bad\" }";

        // First respond to oauth/access_token with a successful token response,
        // then return an error for the bookmarks/list call so SendAsync throws InstapaperApiException.
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("oauth_token=abc&oauth_token_secret=def", Encoding.UTF8, "text/plain")
        };
        var errorResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(errorJson, Encoding.UTF8, "application/json")
        };

        var sut = CreateClient(req => req.RequestUri!.AbsolutePath.Contains("oauth/access_token") ? tokenResponse : errorResponse);

        await Assert.ThrowsAsync<InstapaperApiException>(() => sut.ListBookmarksAsync(null, null, 1));
    }

    [Fact]
    public async Task SendAsync_ThrowsJsonException_OnEmptyResponse()
    {
        // Ensure authentication succeeds first
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("oauth_token=abc&oauth_token_secret=def", Encoding.UTF8, "text/plain")
        };
        var emptyResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };

        var sut = CreateClient(req => req.RequestUri!.AbsolutePath.Contains("oauth/access_token") ? tokenResponse : emptyResponse);

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(() => sut.ListBookmarksAsync(null, null, 1));
    }

    [Fact]
    public async Task EnsureAuthenticated_PopulatesToken_WhenOauthResponseReturned()
    {
        // Simulate oauth/access_token response then a subsequent bookmarks/list call
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("oauth_token=abc&oauth_token_secret=def", Encoding.UTF8, "text/plain")
        };
        var listResponseJson = "[ { \"type\": \"bookmark\", \"bookmark_id\": 5, \"url\": \"http://z\", \"title\": \"Z\", \"folder_id\": 0 } ]";
        var listResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(listResponseJson, Encoding.UTF8, "application/json")
        };

        var sut = CreateClient(req => req.RequestUri!.AbsolutePath.Contains("oauth/access_token") ? tokenResponse : listResponse);

        var results = await sut.ListBookmarksAsync(null, null, 1);

        Assert.Single(results);
        Assert.Equal(5, results.First().BookmarkId);
    }
}
