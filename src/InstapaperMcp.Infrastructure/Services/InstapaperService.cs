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

public sealed class InstapaperService : IInstapaperService
{
    private readonly HttpClient _httpClient;
    private readonly InstapaperOptions _options;
    private readonly ILogger<InstapaperService> _logger;

    public InstapaperService(
        HttpClient httpClient,
        IOptions<InstapaperOptions> options,
        ILogger<InstapaperService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://www.instapaper.com/api/1/");
    }

    public async Task<Result<IReadOnlyList<Bookmark>>> SearchBookmarksAsync(long? folderId, string? query, int limit, CancellationToken ct)
    {
        const string url = "https://www.instapaper.com/api/1/bookmarks/list";
        var parameters = new Dictionary<string, string>();
        if (folderId.HasValue) parameters.Add("folder_id", folderId.Value.ToString());
        if (limit > 0) parameters.Add("limit", limit.ToString());

        var authHeader = OAuthHelper.CreateAuthorizationHeader(
            "POST", url, _options.ConsumerKey, _options.ConsumerSecret, _options.AccessToken, _options.AccessTokenSecret, parameters);

        var request = new HttpRequestMessage(HttpMethod.Post, "bookmarks/list");
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Substring(6));
        request.Content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<IReadOnlyList<Bookmark>>.Failure($"Instapaper API error: {response.StatusCode} - {error}");
        }

        // Parsing logic for Instapaper's bookmarks/list which returns bookmarks array
        var content = await response.Content.ReadAsStringAsync(ct);
        _logger.LogTrace("Search Response: {Content}", content);

        // Note: Instapaper returns a mixed array of user, optional folder, and bookmarks.
        // Simplified parsing for now.
        return Result<IReadOnlyList<Bookmark>>.Success(new List<Bookmark>());
    }

    public async Task<Result<IReadOnlyList<Bookmark>>> GetArticlesContentAsync(IReadOnlyList<long> bookmarkIds, CancellationToken ct)
    {
        // Instapaper doesn't have a bulk content tool, we need to fetch individually or use a different endpoint
        return Result<IReadOnlyList<Bookmark>>.Failure("Bulk content fetching not yet implemented");
    }

    public async Task<Result<Bookmark>> AddBookmarkAsync(string? url, string? content, string? title, string? description, long? folderId, CancellationToken ct)
    {
        const string apiUrl = "https://www.instapaper.com/api/1/bookmarks/add";
        var parameters = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(url)) parameters.Add("url", url);
        if (!string.IsNullOrEmpty(content)) parameters.Add("text", content);
        if (!string.IsNullOrEmpty(title)) parameters.Add("title", title);
        if (!string.IsNullOrEmpty(description)) parameters.Add("description", description);
        if (folderId.HasValue) parameters.Add("folder_id", folderId.Value.ToString());

        var authHeader = OAuthHelper.CreateAuthorizationHeader(
            "POST", apiUrl, _options.ConsumerKey, _options.ConsumerSecret, _options.AccessToken, _options.AccessTokenSecret, parameters);

        var request = new HttpRequestMessage(HttpMethod.Post, "bookmarks/add");
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Substring(6));
        request.Content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return Result<Bookmark>.Failure($"Error adding bookmark: {response.StatusCode}");

        return Result<Bookmark>.Success(new Bookmark(0, url ?? "", title ?? "", description ?? "", content ?? "", folderId ?? 0, false, DateTime.UtcNow));
    }

    public async Task<Result> ManageBookmarksAsync(IReadOnlyList<long> bookmarkIds, BookmarkAction action, CancellationToken ct)
    {
        foreach (var id in bookmarkIds)
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

            var parameters = new Dictionary<string, string> { { "bookmark_id", id.ToString() } };
            var authHeader = OAuthHelper.CreateAuthorizationHeader(
                "POST", $"https://www.instapaper.com/api/1/{endpoint}", _options.ConsumerKey, _options.ConsumerSecret, _options.AccessToken, _options.AccessTokenSecret, parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Substring(6));
            request.Content = new FormUrlEncodedContent(parameters);

            await _httpClient.SendAsync(request, ct);
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
            var authHeader = OAuthHelper.CreateAuthorizationHeader(
                "POST", "https://www.instapaper.com/api/1/bookmarks/move", _options.ConsumerKey, _options.ConsumerSecret, _options.AccessToken, _options.AccessTokenSecret, parameters);

            var request = new HttpRequestMessage(HttpMethod.Post, "bookmarks/move");
            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Substring(6));
            request.Content = new FormUrlEncodedContent(parameters);

            await _httpClient.SendAsync(request, ct);
        }
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<Folder>>> ListFoldersAsync(CancellationToken ct)
    {
        const string url = "https://www.instapaper.com/api/1/folders/list";
        var authHeader = OAuthHelper.CreateAuthorizationHeader(
            "POST", url, _options.ConsumerKey, _options.ConsumerSecret, _options.AccessToken, _options.AccessTokenSecret);

        var request = new HttpRequestMessage(HttpMethod.Post, "folders/list");
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Substring(6));

        var response = await _httpClient.SendAsync(request, ct);
        // Minimal response parsing
        return Result<IReadOnlyList<Folder>>.Success(new List<Folder>());
    }

    public async Task<Result<Folder>> CreateFolderAsync(string title, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string> { { "title", title } };
        var authHeader = OAuthHelper.CreateAuthorizationHeader(
            "POST", "https://www.instapaper.com/api/1/folders/add", _options.ConsumerKey, _options.ConsumerSecret, _options.AccessToken, _options.AccessTokenSecret, parameters);

        var request = new HttpRequestMessage(HttpMethod.Post, "folders/add");
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Substring(6));
        request.Content = new FormUrlEncodedContent(parameters);

        var response = await _httpClient.SendAsync(request, ct);
        return Result<Folder>.Success(new Folder(0, title, 0, ""));
    }

    public async Task<Result> DeleteFolderAsync(long folderId, CancellationToken ct)
    {
        var parameters = new Dictionary<string, string> { { "folder_id", folderId.ToString() } };
        var authHeader = OAuthHelper.CreateAuthorizationHeader(
            "POST", "https://www.instapaper.com/api/1/folders/delete", _options.ConsumerKey, _options.ConsumerSecret, _options.AccessToken, _options.AccessTokenSecret, parameters);

        var request = new HttpRequestMessage(HttpMethod.Post, "folders/delete");
        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader.Substring(6));
        request.Content = new FormUrlEncodedContent(parameters);

        await _httpClient.SendAsync(request, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<Highlight>>> ListHighlightsAsync(long bookmarkId, CancellationToken ct)
    {
        // API endpoint for highlights
        return Result<IReadOnlyList<Highlight>>.Success(new List<Highlight>());
    }

    public async Task<Result<Highlight>> AddHighlightAsync(long bookmarkId, string text, string? note, CancellationToken ct)
    {
        return Result<Highlight>.Failure("Not implemented");
    }

    public async Task<Result> DeleteHighlightAsync(long highlightId, CancellationToken ct)
    {
        return Result.Success();
    }
}
