using System.Net.Http.Headers;
using System.Text.Json;
using InstapaperMcp.Application.Interfaces;
using InstapaperMcp.Domain.Common;
using InstapaperMcp.Domain.Entities;
using InstapaperMcp.Infrastructure.Auth;
using InstapaperMcp.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InstapaperMcp.Infrastructure.Services;

public sealed class InstapaperService(
    HttpClient httpClient,
    IOptions<InstapaperOptions> options,
    ILogger<InstapaperService> logger) : IInstapaperService
{
    private readonly InstapaperOptions _options = options.Value;

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method,
        string endpoint,
        Dictionary<string, string>? parameters = null,
        CancellationToken ct = default)
    {
        var url = $"https://www.instapaper.com/api/1/{endpoint}";
        var authHeader = OAuthHelper.CreateAuthorizationHeader(
            method.Method, url, _options.ConsumerKey, _options.ConsumerSecret, _options.AccessToken, _options.AccessTokenSecret, parameters);

        var request = new HttpRequestMessage(method, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader[6..]);

        if (parameters is { Count: > 0 })
        {
            request.Content = new FormUrlEncodedContent(parameters);
        }

        return await httpClient.SendAsync(request, ct);
    }

    public async Task<Result<IReadOnlyList<Bookmark>>> SearchBookmarksAsync(long? folderId, string? query, int limit, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>();
        if (folderId.HasValue) parameters.Add("folder_id", folderId.Value.ToString());
        if (limit > 0) parameters.Add("limit", limit.ToString());

        var response = await SendRequestAsync(HttpMethod.Post, "bookmarks/list", parameters, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<IReadOnlyList<Bookmark>>.Failure($"Instapaper API error: {response.StatusCode} - {error}");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        logger.LogTrace("Search Response: {Content}", content);

        return Result<IReadOnlyList<Bookmark>>.Success([]);
    }

    public Task<Result<IReadOnlyList<Bookmark>>> GetArticlesContentAsync(IReadOnlyList<long> bookmarkIds, CancellationToken ct)
    {
        return Task.FromResult(Result<IReadOnlyList<Bookmark>>.Failure("Bulk content fetching not yet implemented"));
    }

    public async Task<Result<Bookmark>> AddBookmarkAsync(string? url, string? content, string? title, string? description, long? folderId, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(url)) parameters.Add("url", url);
        if (!string.IsNullOrEmpty(content)) parameters.Add("text", content);
        if (!string.IsNullOrEmpty(title)) parameters.Add("title", title);
        if (!string.IsNullOrEmpty(description)) parameters.Add("description", description);
        if (folderId.HasValue) parameters.Add("folder_id", folderId.Value.ToString());

        var response = await SendRequestAsync(HttpMethod.Post, "bookmarks/add", parameters, ct);
        if (!response.IsSuccessStatusCode)
            return Result<Bookmark>.Failure($"Error adding bookmark: {response.StatusCode}");

        return Result<Bookmark>.Success(new Bookmark(0, url ?? "", title ?? "", description ?? "", content ?? "", folderId ?? 0, false, DateTime.UtcNow));
    }

    public async Task<Result> ManageBookmarksAsync(IReadOnlyList<long> bookmarkIds, BookmarkAction action, CancellationToken ct)
    {
        var endpoint = action switch
        {
            BookmarkAction.Archive => "bookmarks/archive",
            BookmarkAction.Unarchive => "bookmarks/unarchive",
            BookmarkAction.Delete => "bookmarks/delete",
            BookmarkAction.Star => "bookmarks/star",
            BookmarkAction.Unstar => "bookmarks/unstar",
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        foreach (var id in bookmarkIds)
        {
            var parameters = new Dictionary<string, string> { { "bookmark_id", id.ToString() } };
            await SendRequestAsync(HttpMethod.Post, endpoint, parameters, ct);
        }

        return Result.Success();
    }

    public async Task<Result> MoveBookmarksAsync(IReadOnlyList<long> bookmarkIds, long folderId, CancellationToken ct)
    {
        foreach (var id in bookmarkIds)
        {
            var parameters = new Dictionary<string, string>
            {
                { "bookmark_id", id.ToString() },
                { "folder_id", folderId.ToString() }
            };
            await SendRequestAsync(HttpMethod.Post, "bookmarks/move", parameters, ct);
        }
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<Folder>>> ListFoldersAsync(CancellationToken ct)
    {
        var response = await SendRequestAsync(HttpMethod.Post, "folders/list", null, ct);
        return Result<IReadOnlyList<Folder>>.Success([]);
    }

    public async Task<Result<Folder>> CreateFolderAsync(string title, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string> { { "title", title } };
        await SendRequestAsync(HttpMethod.Post, "folders/add", parameters, ct);
        return Result<Folder>.Success(new Folder(0, title, 0, ""));
    }

    public async Task<Result> DeleteFolderAsync(long folderId, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string> { { "folder_id", folderId.ToString() } };
        await SendRequestAsync(HttpMethod.Post, "folders/delete", parameters, ct);
        return Result.Success();
    }

    public Task<Result<IReadOnlyList<Highlight>>> ListHighlightsAsync(long bookmarkId, CancellationToken ct)
    {
        return Task.FromResult(Result<IReadOnlyList<Highlight>>.Success([]));
    }

    public Task<Result<Highlight>> AddHighlightAsync(long bookmarkId, string text, string? note, CancellationToken ct)
    {
        return Task.FromResult(Result<Highlight>.Failure("Not implemented"));
    }

    public Task<Result> DeleteHighlightAsync(long highlightId, CancellationToken ct)
    {
        return Task.FromResult(Result.Success());
    }
}
