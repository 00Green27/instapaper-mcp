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
    private static string? _cachedToken;
    private static string? _cachedTokenSecret;

    private async Task EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_cachedToken ?? _options.AccessToken)) return;

        if (string.IsNullOrEmpty(_options.Username) || string.IsNullOrEmpty(_options.Password))
        {
            logger.LogWarning("Authentication tokens are missing and no Username/Password provided for xAuth.");
            return;
        }

        logger.LogInformation("Attempting xAuth for user {Username}", _options.Username);
        var url = "https://www.instapaper.com/api/1/oauth/access_token";
        var parameters = new Dictionary<string, string>
        {
            { "x_auth_username", _options.Username },
            { "x_auth_password", _options.Password },
            { "x_auth_mode", "client_auth" }
        };

        // For xAuth, we sign with just Consumer Key/Secret
        var authHeader = OAuthHelper.CreateAuthorizationHeader(
            "POST", url, _options.ConsumerKey, _options.ConsumerSecret, null, null, parameters);

        var request = new HttpRequestMessage(HttpMethod.Post, "oauth/access_token");
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader[6..]);
        request.Content = new FormUrlEncodedContent(parameters);

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"xAuth failed: {response.StatusCode} - {error}");
        }

        var result = await response.Content.ReadAsStringAsync(ct);
        var parts = result.Split('&')
            .Select(p => p.Split('='))
            .ToDictionary(p => p[0], p => p[1]);

        _cachedToken = Uri.UnescapeDataString(parts["oauth_token"]);
        _cachedTokenSecret = Uri.UnescapeDataString(parts["oauth_token_secret"]);
        logger.LogInformation("xAuth successful, tokens cached.");
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method,
        string endpoint,
        Dictionary<string, string>? parameters = null,
        CancellationToken ct = default)
    {
        await EnsureAuthenticatedAsync(ct);

        var url = $"https://www.instapaper.com/api/1/{endpoint}";
        var token = _cachedToken ?? _options.AccessToken;
        var secret = _cachedTokenSecret ?? _options.AccessTokenSecret;

        var authHeader = OAuthHelper.CreateAuthorizationHeader(
            method.Method, url, _options.ConsumerKey, _options.ConsumerSecret, token, secret, parameters);

        var request = new HttpRequestMessage(method, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader[6..]);

        if (parameters is { Count: > 0 })
        {
            request.Content = new FormUrlEncodedContent(parameters);
        }

        return await httpClient.SendAsync(request, ct);
    }

    public async Task<Result<IReadOnlyList<Bookmark>>> SearchBookmarksAsync(long? folderId, string? query, int limit,
        CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>();
        if (folderId.HasValue) parameters.Add("folder_id", folderId.Value.ToString());

        // If we need to filter safely, we must fetch more items than requested.
        // Instapaper max limit is 500.
        var fetchLimit = string.IsNullOrWhiteSpace(query) ? limit : 500;
        if (fetchLimit > 0) parameters.Add("limit", fetchLimit.ToString());

        var response = await SendRequestAsync(HttpMethod.Post, "bookmarks/list", parameters, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<IReadOnlyList<Bookmark>>.Failure($"Instapaper API error: {response.StatusCode} - {error}");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(content);
        var bookmarks = new List<Bookmark>();

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            if (element.GetProperty("type").GetString() != "bookmark") continue;

            bookmarks.Add(new Bookmark(
                Id: element.GetProperty("bookmark_id").GetInt64(),
                Url: element.GetProperty("url").GetString() ?? "",
                Title: element.GetProperty("title").GetString() ?? "",
                Description: element.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                Content: "", // Content is not returned in list
                FolderId: element.TryGetProperty("folder_id", out var fid) && fid.ValueKind == JsonValueKind.Number
                    ? fid.GetInt64()
                    : 0,
                IsStarred: element.GetProperty("starred").GetString() == "1",
                UpdatedAt: DateTimeOffset.FromUnixTimeSeconds(element.GetProperty("time").GetInt64()).DateTime
            ));
        }

        // Perform client-side filtering if query is present
        if (!string.IsNullOrWhiteSpace(query))
        {
            bookmarks = bookmarks
                .Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            b.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            b.Url.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToList();
        }

        return Result<IReadOnlyList<Bookmark>>.Success(bookmarks);
    }

    public async Task<Result<IReadOnlyList<Bookmark>>> GetArticlesContentAsync(IReadOnlyList<long> bookmarkIds,
        CancellationToken ct)
    {
        var bookmarks = new List<Bookmark>();
        foreach (var id in bookmarkIds)
        {
            var parameters = new Dictionary<string, string> { { "bookmark_id", id.ToString() } };
            var response = await SendRequestAsync(HttpMethod.Post, "bookmarks/get_text", parameters, ct);

            if (response.IsSuccessStatusCode)
            {
                var text = await response.Content.ReadAsStringAsync(ct);
                bookmarks.Add(new Bookmark(id, "", "", "", text, 0, false, DateTime.UtcNow));
            }
            else
            {
                logger.LogWarning("Failed to fetch content for bookmark {Id}: {Status}", id, response.StatusCode);
            }
        }

        return Result<IReadOnlyList<Bookmark>>.Success(bookmarks);
    }

    public async Task<Result<Bookmark>> AddBookmarkAsync(string? url, string? content, string? title,
        string? description, long? folderId, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(url)) parameters.Add("url", url);
        if (!string.IsNullOrEmpty(content)) parameters.Add("text", content);
        if (!string.IsNullOrEmpty(title)) parameters.Add("title", title);
        if (!string.IsNullOrEmpty(description)) parameters.Add("description", description);
        if (folderId.HasValue) parameters.Add("folder_id", folderId.Value.ToString());

        var response = await SendRequestAsync(HttpMethod.Post, "bookmarks/add", parameters, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<Bookmark>.Failure($"Error adding bookmark: {response.StatusCode} - {error}");
        }

        var resContent = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(resContent);
        var element = doc.RootElement.EnumerateArray().First(x => x.GetProperty("type").GetString() == "bookmark");

        return Result<Bookmark>.Success(new Bookmark(
            Id: element.GetProperty("bookmark_id").GetInt64(),
            Url: element.GetProperty("url").GetString() ?? "",
            Title: element.GetProperty("title").GetString() ?? "",
            Description: element.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
            Content: "", // Content not returned here
            FolderId: element.TryGetProperty("folder_id", out var fid) && fid.ValueKind == JsonValueKind.Number
                ? fid.GetInt64()
                : 0,
            IsStarred: element.GetProperty("starred").GetString() == "1",
            UpdatedAt: DateTimeOffset.FromUnixTimeSeconds(element.GetProperty("time").GetInt64()).DateTime
        ));
    }

    public async Task<Result> ManageBookmarksAsync(IReadOnlyList<long> bookmarkIds, BookmarkAction action,
        CancellationToken ct)
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
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<IReadOnlyList<Folder>>.Failure($"Failed to list folders: {error}");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(content);
        var folders = doc.RootElement.EnumerateArray()
            .Select(element => new Folder(
                Id: element.GetProperty("folder_id").GetInt64(),
                Title: element.GetProperty("title").GetString() ?? "",
                Position: element.GetProperty("position").GetInt32(),
                Slug: element.GetProperty("slug").GetString() ?? ""
            ))
            .ToList();

        return Result<IReadOnlyList<Folder>>.Success(folders);
    }

    public async Task<Result<Folder>> CreateFolderAsync(string title, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string> { { "title", title } };
        var response = await SendRequestAsync(HttpMethod.Post, "folders/add", parameters, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<Folder>.Failure($"Failed to create folder: {error}");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(content);
        var element = doc.RootElement.EnumerateArray().First(x => x.GetProperty("type").GetString() == "folder");

        return Result<Folder>.Success(new Folder(
            Id: element.GetProperty("folder_id").GetInt64(),
            Title: element.GetProperty("title").GetString() ?? "",
            Position: element.GetProperty("position").GetInt32(),
            Slug: element.GetProperty("slug").GetString() ?? ""
        ));
    }

    public async Task<Result> DeleteFolderAsync(long folderId, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string> { { "folder_id", folderId.ToString() } };
        await SendRequestAsync(HttpMethod.Post, "folders/delete", parameters, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<Highlight>>> ListHighlightsAsync(long bookmarkId, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string> { { "bookmark_id", bookmarkId.ToString() } };
        var response = await SendRequestAsync(HttpMethod.Post, "bookmarks/get_highlights", parameters, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<IReadOnlyList<Highlight>>.Failure($"Failed to list highlights: {error}");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(content);
        var highlights = doc.RootElement.EnumerateArray()
            .Select(element => new Highlight(
                Id: element.GetProperty("highlight_id").GetInt64(),
                BookmarkId: element.GetProperty("bookmark_id").GetInt64(),
                Text: element.GetProperty("text").GetString() ?? "",
                Note: element.TryGetProperty("note", out var n) ? n.GetString() ?? "" : "",
                CreatedAt: DateTimeOffset.FromUnixTimeSeconds(element.GetProperty("time").GetInt64()).DateTime
            ))
            .ToList();

        return Result<IReadOnlyList<Highlight>>.Success(highlights);
    }

    public async Task<Result<Highlight>> AddHighlightAsync(long bookmarkId, string text, string? note,
        CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>
        {
            { "bookmark_id", bookmarkId.ToString() },
            { "text", text }
        };
        if (!string.IsNullOrEmpty(note)) parameters.Add("note", note);

        var response = await SendRequestAsync(HttpMethod.Post, "bookmarks/add_highlight", parameters, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<Highlight>.Failure($"Failed to add highlight: {error}");
        }

        var content = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(content);
        var element = doc.RootElement.EnumerateArray().First(x => x.GetProperty("type").GetString() == "highlight");

        return Result<Highlight>.Success(new Highlight(
            Id: element.GetProperty("highlight_id").GetInt64(),
            BookmarkId: element.GetProperty("bookmark_id").GetInt64(),
            Text: element.GetProperty("text").GetString() ?? "",
            Note: element.TryGetProperty("note", out var n) ? n.GetString() ?? "" : "",
            CreatedAt: DateTimeOffset.FromUnixTimeSeconds(element.GetProperty("time").GetInt64()).DateTime
        ));
    }

    public Task<Result> DeleteHighlightAsync(long highlightId, CancellationToken ct)
    {
        return Task.FromResult(Result.Success());
    }
}
