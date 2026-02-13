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

    public async Task<IReadOnlyList<Bookmark>> SearchBookmarksAsync(
        string? query,
        long? folderId,
        int? limit,
        CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string>();
        if (query != null)
            parameters["search"] = query;

        if (folderId.HasValue)
            parameters["folder_id"] = folderId.Value.ToString();

        if (!limit.HasValue || limit <= 0)
            limit = DefaultLimit;

        limit = Math.Min(limit.Value, 500);

        parameters["limit"] = limit.Value.ToString();

        var items = await SendAsync<List<InstapaperItem>>(
            HttpMethod.Post,
            "bookmarks/list",
            parameters,
            ct);

        return items.OfType<Bookmark>().ToArray();
    }

    // get_article_content (single and bulk)
    public async Task<string> GetArticleContentAsync(long bookmarkId, CancellationToken ct = default)
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

    public async Task<IReadOnlyList<Folder>> ListFoldersAsync(CancellationToken ct = default)
    {
        var items = await SendAsync<List<InstapaperItem>>(
            HttpMethod.Get,
            "folders/list",
            parameters: null,
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
            var payload = await new StreamReader(stream).ReadToEndAsync(ct);
            throw InstapaperApiException.FromResponse(response, payload);
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