using System.Text.Json;
using InstapaperMcp.Application.Interfaces;
using InstapaperMcp.Domain.Common;
using Microsoft.Extensions.Logging;

namespace InstapaperMcp.Api.Handlers;

public sealed class McpHandler(
    IInstapaperService instapaperService)
{
    public async Task<object?> HandleAsync(string method, JsonElement? parameters, CancellationToken ct)
    {
        return method switch
        {
            "listTools" => ListTools(),
            "callTool" => await CallToolAsync(parameters, ct),
            "listResources" => ListResources(),
            "readResource" => await ReadResourceAsync(parameters, ct),
            _ => throw new NotSupportedException($"Method '{method}' is not supported.")
        };
    }

    private static object ListTools() => new
    {
        tools = new[]
        {
            new { name = "search_bookmarks", description = "List or search bookmarks. Defaults to unread folder." },
            new { name = "get_article_content", description = "Fetch text content for one or more bookmarks." },
            new { name = "add_bookmark", description = "Add a new bookmark or note." },
            new
            {
                name = "manage_bookmarks",
                description =
                    "Archive, unarchive, delete, star, or unstar bookmarks. Action is applied to all provided IDs."
            },
            new { name = "move_bookmarks", description = "Move bookmarks to a different folder." },
            new { name = "manage_folders", description = "List, create, or delete folders." },
            new { name = "manage_highlights", description = "List, add, or delete highlights." }
        }
    };

    private async Task<object> CallToolAsync(JsonElement? parameters, CancellationToken ct)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        var name = parameters.Value.GetProperty("name").GetString();
        var args = parameters.Value.GetProperty("arguments");

        return name switch
        {
            "search_bookmarks" => await SearchBookmarksAsync(args, ct),
            "add_bookmark" => await AddBookmarkAsync(args, ct),
            "manage_bookmarks" => await ManageBookmarksAsync(args, ct),
            "move_bookmarks" => await MoveBookmarksAsync(args, ct),
            "manage_folders" => await ManageFoldersAsync(args, ct),
            "manage_highlights" => await ManageHighlightsAsync(args, ct),
            _ => throw new NotSupportedException($"Tool '{name}' is not supported.")
        };
    }

    private async Task<object> SearchBookmarksAsync(JsonElement args, CancellationToken ct)
    {
        var folderId = args.TryGetProperty("folder_id", out var f) ? f.GetInt64() : (long?)null;
        var query = args.TryGetProperty("query", out var q) ? q.GetString() : null;
        var limit = args.TryGetProperty("limit", out var l) ? l.GetInt32() : 10;

        var result = await instapaperService.SearchBookmarksAsync(folderId, query, limit, ct);
        return FormatResult(result);
    }

    private async Task<object> AddBookmarkAsync(JsonElement args, CancellationToken ct)
    {
        var url = args.TryGetProperty("url", out var u) ? u.GetString() : null;
        var content = args.TryGetProperty("content", out var c) ? c.GetString() : null;
        var title = args.TryGetProperty("title", out var t) ? t.GetString() : null;
        var description = args.TryGetProperty("description", out var d) ? d.GetString() : null;
        var folderId = args.TryGetProperty("folder_id", out var f) ? f.GetInt64() : (long?)null;

        var result = await instapaperService.AddBookmarkAsync(url, content, title, description, folderId, ct);
        return FormatResult(result);
    }

    private async Task<object> ManageBookmarksAsync(JsonElement args, CancellationToken ct)
    {
        var ids = args.GetProperty("bookmark_ids").EnumerateArray().Select(x => x.GetInt64()).ToList();
        var actionStr = args.GetProperty("action").GetString();
        var action = Enum.Parse<BookmarkAction>(actionStr!, true);

        var result = await instapaperService.ManageBookmarksAsync(ids, action, ct);
        return FormatResult(result);
    }

    private async Task<object> MoveBookmarksAsync(JsonElement args, CancellationToken ct)
    {
        var ids = args.GetProperty("bookmark_ids").EnumerateArray().Select(x => x.GetInt64()).ToList();
        var folderId = args.GetProperty("folder_id").GetInt64();

        var result = await instapaperService.MoveBookmarksAsync(ids, folderId, ct);
        return FormatResult(result);
    }

    private async Task<object> ManageFoldersAsync(JsonElement args, CancellationToken ct)
    {
        var action = args.GetProperty("action").GetString();
        return action switch
        {
            "list" => FormatResult(await instapaperService.ListFoldersAsync(ct)),
            "create" => FormatResult(
                await instapaperService.CreateFolderAsync(args.GetProperty("title").GetString()!, ct)),
            "delete" => FormatResult(
                await instapaperService.DeleteFolderAsync(args.GetProperty("folder_id").GetInt64(), ct)),
            _ => throw new NotSupportedException($"Action '{action}' is not supported for manage_folders.")
        };
    }

    private async Task<object> ManageHighlightsAsync(JsonElement args, CancellationToken ct)
    {
        var action = args.GetProperty("action").GetString();
        return action switch
        {
            "list" => FormatResult(
                await instapaperService.ListHighlightsAsync(args.GetProperty("bookmark_id").GetInt64(), ct)),
            "add" => FormatResult(await instapaperService.AddHighlightAsync(args.GetProperty("bookmark_id").GetInt64(),
                args.GetProperty("text").GetString()!, args.TryGetProperty("note", out var n) ? n.GetString() : null,
                ct)),
            "delete" => FormatResult(
                await instapaperService.DeleteHighlightAsync(args.GetProperty("highlight_id").GetInt64(), ct)),
            _ => throw new NotSupportedException($"Action '{action}' is not supported for manage_highlights.")
        };
    }

    private static object ListResources() => new
    {
        resources = new[]
        {
            new
            {
                uri = "instapaper://folders", name = "Application folders", description = "List of all user folders"
            },
            new
            {
                uri = "instapaper://bookmarks/unread", name = "Unread bookmarks",
                description = "Default bucket for new items"
            }
        }
    };

    private async Task<object> ReadResourceAsync(JsonElement? parameters, CancellationToken ct)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        var uri = parameters.Value.GetProperty("uri").GetString();

        if (uri == "instapaper://folders")
        {
            var result = await instapaperService.ListFoldersAsync(ct);
            return new { contents = new[] { new { uri, text = JsonSerializer.Serialize(result) } } };
        }

        return new { contents = Array.Empty<object>() };
    }

    private static object FormatResult(Result result) => result.IsSuccess
        ? new { content = new[] { new { type = "text", text = "Operation completed successfully." } } }
        : new { isError = true, content = new[] { new { type = "text", text = result.Error } } };

    private static object FormatResult<T>(Result<T> result) => result.IsSuccess
        ? new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = JsonSerializer.Serialize(result.Value, new JsonSerializerOptions { WriteIndented = true })
                }
            }
        }
        : new { isError = true, content = new[] { new { type = "text", text = result.Error } } };
}
