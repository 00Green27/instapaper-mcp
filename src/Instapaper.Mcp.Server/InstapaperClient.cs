using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Instapaper.Mcp.Server.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Instapaper.Mcp.Server;

public sealed class InstapaperClient : IInstapaperClient
{
    private const int DefaultLimit = 100;

    private readonly InstapaperOptions _options;
    private readonly HttpClient _httpClient;
    private readonly IOAuth1SignatureGenerator _signatureGenerator;
    private readonly ILogger<InstapaperClient> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private static string? _cachedToken;
    private static string? _cachedTokenSecret;

    public InstapaperClient(
        HttpClient httpClient,
        IOAuth1SignatureGenerator signatureGenerator,
        IOptions<InstapaperOptions> options,
        ILogger<InstapaperClient> logger)
    {
        _httpClient = httpClient;
        _signatureGenerator = signatureGenerator;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Bookmark>> ListBookmarksAsync(
        string? query,
        string? folderId,
        int? limit,
        CancellationToken ct = default)
    {
        const int MaxApiLimit = 500;
        const int MaxPages = 10;

        var parameters = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(folderId))
            parameters["folder_id"] = folderId.ToString();

        var lowerQuery = !string.IsNullOrWhiteSpace(query) ? query!.ToLowerInvariant() : null;

        if (!limit.HasValue || limit <= 0)
            limit = DefaultLimit;

        int effectiveLimit = Math.Min(limit.Value, MaxApiLimit * MaxPages);

        var results = new List<Bookmark>();
        var seenIds = new List<string>();
        for (int page = 0; page < MaxPages; page++)
        {
            parameters["limit"] = MaxApiLimit.ToString();

            if (seenIds.Count > 0)
            {
                parameters["have"] = string.Join(",", seenIds);
            }
            var items = await SendAsync<List<InstapaperItem>>(
                HttpMethod.Post,
                "bookmarks/list",
                parameters,
                ct);

            if (items == null) break;

            var newBookmarks = items.OfType<Bookmark>().ToArray();

            if (newBookmarks.Length == 0) break;

            seenIds.AddRange(newBookmarks.Select(b => b.BookmarkId.ToString()));

            var candidates = lowerQuery is not null
                ? newBookmarks.Where(b =>
                    b.Title?.ToLowerInvariant().Contains(lowerQuery, StringComparison.InvariantCultureIgnoreCase) ?? false)
                : newBookmarks.AsEnumerable();

            results.AddRange(candidates);

            if (results.Count >= effectiveLimit) break;
        }

        return results.Take(limit.Value).ToArray();
    }

    public async Task<Bookmark> AddBookmarkAsync(
        string? url = null,
        string? title = null,
        string? description = null,
        long? folderId = null,
        string? content = null,
        bool resolveFinalUrl = true,
        bool archiveOnAdd = false,
        CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string>();
        if (url != null)
            parameters["url"] = url;
        if (title != null)
            parameters["title"] = title;
        if (description != null)
            parameters["description"] = description;
        if (folderId.HasValue)
            parameters["folder_id"] = folderId.Value.ToString();
        if (content != null)
            parameters["content"] = content;
        parameters["resolve_final_url"] = resolveFinalUrl ? "1" : "0";
        parameters["archived"] = archiveOnAdd ? "1" : "0";

        if (url == null && content != null)
        {
            parameters["is_private_from_source"] = "note";
        }

        var items = await SendAsync<List<InstapaperItem>>(
            HttpMethod.Post,
            "bookmarks/add",
            parameters,
            ct);

        return items.OfType<Bookmark>().First();
    }

    public async Task<string> GetBookmarkContentAsync(long bookmarkId, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["bookmark_id"] = bookmarkId.ToString()
        };

        return await SendAsync<string>(
            HttpMethod.Post,
            "bookmarks/get_text",
            parameters
            , ct);
    }

    public async Task<IReadOnlyDictionary<long, string>> GetBookmarkContentsAsync(IEnumerable<long> bookmarkIds, CancellationToken ct = default)
    {
        var tasks = bookmarkIds.Select(async id => (id, content: await GetBookmarkContentAsync(id, ct)));
        var results = await Task.WhenAll(tasks);
        return new Dictionary<long, string>(results.ToDictionary(x => x.id, x => x.content));
    }

    public async Task<Bookmark> ManageBookmarksAsync(long bookmarkId, BookmarkAction action, CancellationToken ct = default)
    {
        string path = action switch
        {
            BookmarkAction.Archive => "bookmarks/archive",
            BookmarkAction.Unarchive => "bookmarks/unarchive",
            BookmarkAction.Delete => "bookmarks/delete",
            BookmarkAction.Mark => "bookmarks/star",
            BookmarkAction.Unmark => "bookmarks/unstar",
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        var parameters = new Dictionary<string, string>
        {
            ["bookmark_id"] = bookmarkId.ToString()
        };

        var items = await SendAsync<List<InstapaperItem>>(HttpMethod.Post, path, parameters, ct);

        return items.OfType<Bookmark>().First();
    }

    public async Task<IReadOnlyCollection<Bookmark>> ManageBookmarksAsync(IEnumerable<long> bookmarkIds, BookmarkAction action, CancellationToken ct = default)
    {
        var tasks = bookmarkIds.Select(async id => await ManageBookmarksAsync(id, action, ct));
        return await Task.WhenAll(tasks);
    }

    public async Task<Bookmark> MoveBookmarkAsync(long bookmarkId, long folderId, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["bookmark_id"] = bookmarkId.ToString(),
            ["folder_id"] = folderId.ToString()
        };

        var items = await SendAsync<List<InstapaperItem>>(
            HttpMethod.Post,
            "bookmarks/move",
             parameters,
             ct);

        return items.OfType<Bookmark>().First();
    }

    public async Task<IReadOnlyCollection<Bookmark>> MoveBookmarksAsync(IEnumerable<long> bookmarkIds, long folderId, CancellationToken ct = default)
    {
        var tasks = bookmarkIds.Select(async id => await MoveBookmarkAsync(id, folderId, ct));
        return await Task.WhenAll(tasks);
    }

    public async Task<IReadOnlyCollection<Folder>> ListFoldersAsync(CancellationToken ct = default)
    {
        var items = await SendAsync<List<InstapaperItem>>(
            HttpMethod.Get,
            "folders/list",
            parameters: null,
            ct);

        return items.OfType<Folder>().ToArray();
    }
    public async Task<Folder?> SearchFolderAsync(string title, CancellationToken ct = default)
    {
        var items = await SendAsync<List<InstapaperItem>>(
            HttpMethod.Get,
            "folders/list",
            parameters: null,
            ct);

        return items.OfType<Folder>().FirstOrDefault(x => string.Equals(x.Title, title, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Folder> CreateFolderAsync(string title, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["title"] = title
        };

        var items = await SendAsync<List<InstapaperItem>>(
        HttpMethod.Post,
            "folders/add",
            parameters,
            ct);

        return items.OfType<Folder>().First();
    }

    public async Task<bool> DeleteFolderAsync(long folderId, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["folder_id"] = folderId.ToString()
        };

        var items = await SendAsync<List<InstapaperItem>>(
            HttpMethod.Post,
            "folders/delete",
            parameters,
            ct);

        return !items.OfType<Folder>().Any();
    }

    public async Task<IReadOnlyCollection<Folder>> ReorderFoldersAsync((long folderId, int position)[] folderOrders, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["order"] = string.Join(',', folderOrders.Select((f, o) => $"{f}:{o}"))
        };

        var items = await SendAsync<List<InstapaperItem>>(
            HttpMethod.Get,
            "folders/set_order",
            parameters,
            ct);

        return items.OfType<Folder>().ToArray();
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, Dictionary<string, string>? parameters, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        request.RequestUri = new Uri(_httpClient.BaseAddress!, request.RequestUri!);

        if (parameters is not null)
        {
            request.Content = new FormUrlEncodedContent(parameters);
        }

        await SignAsync(request, parameters, ct);

        using var response = await _httpClient.SendAsync(request, ct);
        var stream = await response.Content.ReadAsStreamAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = JsonSerializer.Deserialize(stream, InstapaperJsonContext.Default.Error);
            throw InstapaperApiException.FromResponse(response, error);
        }

        T? result = (T?)JsonSerializer.Deserialize(stream, typeof(List<InstapaperItem>), InstapaperJsonContext.Default);
        return result ?? throw new InvalidOperationException($"Empty response deserializing {typeof(T).Name}");
    }

    private async Task SignAsync(HttpRequestMessage request, Dictionary<string, string>? parameters,
        CancellationToken ct)
    {
        await EnsureAuthenticatedAsync(ct);

        var header = _signatureGenerator.CreateAuthorizationHeader(
            request.Method,
            request.RequestUri!,
            _options.ConsumerKey,
            _options.ConsumerSecret,
            _cachedToken,
            _cachedTokenSecret,
            parameters);

        request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", header[6..]);
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_cachedToken ?? _options.AccessToken)) return;

        await _lock.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrEmpty(_cachedToken)) return;

            var parameters = new Dictionary<string, string>
            {
                ["x_auth_username"] = _options.Username,
                ["x_auth_password"] = _options.Password,
                ["x_auth_mode"] = "client_auth"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "oauth/access_token")
            {
                Content = new FormUrlEncodedContent(parameters)
            };
            request.RequestUri = new Uri(_httpClient.BaseAddress!, request.RequestUri!);

            var authHeader = _signatureGenerator.CreateAuthorizationHeader(
                HttpMethod.Post,
                request.RequestUri!,
                _options.ConsumerKey,
                _options.ConsumerSecret,
                null,
                null,
                parameters);

            request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authHeader[6..]);

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync(ct);
            var parts = result.Split('&')
                .Select(p => p.Split('='))
                .ToDictionary(p => p[0], p => p[1]);

            _cachedToken = Uri.UnescapeDataString(parts["oauth_token"]);
            _cachedTokenSecret = Uri.UnescapeDataString(parts["oauth_token_secret"]);
        }
        finally
        {
            _lock.Release();
        }
    }
}