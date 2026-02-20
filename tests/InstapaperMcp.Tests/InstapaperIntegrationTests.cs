using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Instapaper.Mcp.Server.Tests;

public class InstapaperIntegrationTests
{
    private static bool HasEnv(params string[] names)
    {
        foreach (var n in names)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(n)))
                return false;
        }
        return true;
    }

    private static InstapaperClient CreateFromEnv()
    {
        var consumerKey = Environment.GetEnvironmentVariable("INSTAPAPER_CONSUMER_KEY")!;
        var consumerSecret = Environment.GetEnvironmentVariable("INSTAPAPER_CONSUMER_SECRET")!;
        var username = Environment.GetEnvironmentVariable("INSTAPAPER_USERNAME");
        var password = Environment.GetEnvironmentVariable("INSTAPAPER_PASSWORD");
        var accessToken = Environment.GetEnvironmentVariable("INSTAPAPER_ACCESS_TOKEN");
        var accessTokenSecret = Environment.GetEnvironmentVariable("INSTAPAPER_ACCESS_TOKEN_SECRET");

        var options = Options.Create(new Configuration.InstapaperOptions
        {
            ConsumerKey = consumerKey,
            ConsumerSecret = consumerSecret,
            Username = username ?? string.Empty,
            Password = password ?? string.Empty,
            AccessToken = accessToken,
            AccessTokenSecret = accessTokenSecret
        });

        var http = new HttpClient { BaseAddress = new Uri("https://www.instapaper.com/api/1/") };
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<InstapaperClient>();
        var timeProvider = TimeProvider.System;
        return new InstapaperClient(http, new OAuth1SignatureGenerator(timeProvider), options, logger, timeProvider);
    }

    [Fact]
    public async Task AuthAndListBookmarks_Integration()
    {
        if (!HasEnv("INSTAPAPER_CONSUMER_KEY", "INSTAPAPER_CONSUMER_SECRET"))
            return;

        var client = CreateFromEnv();

        var bookmarks = await client.ListBookmarksAsync(null, null, 1);

        Assert.NotNull(bookmarks);
    }

    [Fact]
    public async Task CreateAndDeleteFolder_Integration()
    {
        if (!HasEnv("INSTAPAPER_CONSUMER_KEY", "INSTAPAPER_CONSUMER_SECRET"))
            return;

        var client = CreateFromEnv();

        var title = "integ-test-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var folder = await client.CreateFolderAsync(title);
        Assert.Equal(title, folder.Title);

        var deleted = await client.DeleteFolderAsync(folder.FolderId);
        Assert.True(deleted);
    }

    [Fact]
    public async Task AddAndDeleteBookmark_Integration_Destructive()
    {
        if (!HasEnv("INSTAPAPER_CONSUMER_KEY", "INSTAPAPER_CONSUMER_SECRET"))
            return;
        if (Environment.GetEnvironmentVariable("INSTAPAPER_RUN_DESTRUCTIVE") != "1")
            return;
        var client = CreateFromEnv();

        var title = "integ-bm-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var bm = await client.AddBookmarkAsync(url: "https://example.com/", title: title);
        Assert.Equal(title, bm.Title);

        var deleted = await client.ManageBookmarksAsync(bm.BookmarkId, BookmarkAction.Delete);
        Assert.Equal(bm.BookmarkId, deleted.BookmarkId);
    }

    [Fact]
    public async Task MoveBookmark_Integration_Destructive()
    {
        if (!HasEnv("INSTAPAPER_CONSUMER_KEY", "INSTAPAPER_CONSUMER_SECRET"))
            return;
        if (Environment.GetEnvironmentVariable("INSTAPAPER_RUN_DESTRUCTIVE") != "1")
            return;
        var client = CreateFromEnv();

        // create a folder and a bookmark, then move the bookmark into the folder and clean up
        var destTitle = "integ-dst-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        var dest = await client.CreateFolderAsync(destTitle);

        var bm = await client.AddBookmarkAsync(url: "https://example.com/", title: "to-move");

        var moved = await client.MoveBookmarkAsync(bm.BookmarkId, dest.FolderId);
        Assert.Equal(dest.FolderId, moved.FolderId);

        // cleanup
        await client.ManageBookmarksAsync(moved.BookmarkId, BookmarkAction.Delete);
        await client.DeleteFolderAsync(dest.FolderId);
    }
}