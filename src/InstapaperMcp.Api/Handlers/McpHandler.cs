using System.Text.Json;
using InstapaperMcp.Application.Interfaces;
using InstapaperMcp.Domain.Common;

namespace InstapaperMcp.Api.Handlers;

public sealed class McpHandler(
    IInstapaperService instapaperService)
{
    public async Task<object?> HandleAsync(string method, JsonElement? parameters, CancellationToken ct)
    {
        return method switch
        {
            "initialize" => Initialize(),
            "notifications/initialized" => null,
            "initialized" => null,
            "tools/list" => ListTools(),
            "tools/call" => await CallToolAsync(parameters, ct),
            "resources/list" => ListResources(),
            "resources/read" => await ReadResourceAsync(parameters, ct),
            "prompts/list" => ListPrompts(),
            "prompts/get" => GetPromptAsync(parameters),
            _ => throw new NotSupportedException($"Method '{method}' is not supported.")
        };
    }

    private static object Initialize() => new
    {
        protocolVersion = "2024-11-05",
        capabilities = new
        {
            tools = new { listChanged = true },
            resources = new { listChanged = true },
            prompts = new { listChanged = true }
        },
        serverInfo = new
        {
            name = "instapaper-mcp",
            version = "1.0.0"
        }
    };

    private static object ListTools() => new
    {
        tools = new object[]
        {
            new
            {
                name = "search_bookmarks",
                description = "List or search bookmarks. Defaults to unread folder.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        folder_id = new { type = "integer", description = "Optional folder ID to search in." },
                        query = new { type = "string", description = "Optional search query." },
                        limit = new
                        {
                            type = "integer", description = "Maximum number of items to return (default 10).",
                            minimum = 1,
                            maximum = 100
                        }
                    }
                }
            },
            new
            {
                name = "add_bookmark",
                description = "Add a new bookmark or note.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        url = new { type = "string", description = "The URL of the bookmark or note." },
                        content = new { type = "string", description = "Optional text content for a note." },
                        title = new { type = "string", description = "Optional title." },
                        description = new { type = "string", description = "Optional description." },
                        folder_id = new { type = "integer", description = "Optional folder ID to add to." }
                    }
                }
            },
            new
            {
                name = "manage_bookmarks",
                description = "Archive, unarchive, delete, star, or unstar bookmarks.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        bookmark_ids = new
                        {
                            type = "array",
                            items = new { type = "integer" },
                            description = "List of bookmark IDs to manage."
                        },
                        action = new
                        {
                            type = "string",
                            @enum = new[] { "archive", "unarchive", "delete", "star", "unstar" },
                            description = "The action to perform."
                        }
                    },
                    required = new[] { "bookmark_ids", "action" }
                }
            },
            new
            {
                name = "move_bookmarks",
                description = "Move bookmarks to a different folder.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        bookmark_ids = new
                        {
                            type = "array",
                            items = new { type = "integer" },
                            description = "List of bookmark IDs to move."
                        },
                        folder_id = new { type = "integer", description = "The target folder ID." }
                    },
                    required = new[] { "bookmark_ids", "folder_id" }
                }
            },
            new
            {
                name = "manage_folders",
                description = "List, create, or delete folders.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new
                        {
                            type = "string",
                            @enum = new[] { "list", "create", "delete" },
                            description = "The action to perform."
                        },
                        title = new { type = "string", description = "The title of the folder (for create action)." },
                        folder_id = new { type = "integer", description = "The folder ID (for delete action)." }
                    },
                    required = new[] { "action" }
                }
            },
            new
            {
                name = "manage_highlights",
                description = "List, add, or delete highlights.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new
                        {
                            type = "string",
                            @enum = new[] { "list", "add", "delete" },
                            description = "The action to perform."
                        },
                        bookmark_id = new
                            { type = "integer", description = "The bookmark ID (for list and add actions)." },
                        text = new { type = "string", description = "The text of the highlight (for add action)." },
                        note = new
                        {
                            type = "string", description = "Optional note for the highlight (for add action)."
                        },
                        highlight_id = new { type = "integer", description = "The highlight ID (for delete action)." }
                    },
                    required = new[] { "action" }
                }
            },
            new
            {
                name = "get_article_content",
                description = "Fetch text content for one or more bookmarks.",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        bookmark_ids = new
                        {
                            type = "array",
                            items = new { type = "integer" },
                            description = "List of bookmark IDs to fetch content for."
                        }
                    },
                    required = new[] { "bookmark_ids" }
                }
            }
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
            "get_article_content" => await GetArticlesContentAsync(args, ct),
            _ => throw new NotSupportedException($"Tool '{name}' is not supported.")
        };
    }

    private async Task<object> GetArticlesContentAsync(JsonElement args, CancellationToken ct)
    {
        var ids = args.GetProperty("bookmark_ids").EnumerateArray().Select(x => x.GetInt64()).ToList();
        var result = await instapaperService.GetArticlesContentAsync(ids, ct);
        return FormatResult(result);
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

    private static object ListPrompts() => new
    {
        prompts = new object[]
        {
            new
            {
                name = "daily_briefing",
                description = "Summarize the latest unread bookmarks.",
                arguments = new object[]
                {
                    new { name = "limit", description = "Number of articles to include (default 5).", required = false }
                }
            },
            new
            {
                name = "research_mode",
                description = "Analyze bookmarks on a specific topic.",
                arguments = new object[]
                {
                    new { name = "topic", description = "The topic to research in your bookmarks.", required = true }
                }
            },
            new
            {
                name = "clean_up_suggestions",
                description = "Identify old or irrelevant bookmarks for archiving.",
                arguments = Array.Empty<object>()
            }
        }
    };

    private object GetPromptAsync(JsonElement? parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        var name = parameters.Value.GetProperty("name").GetString();
        var args = parameters.Value.TryGetProperty("arguments", out var a) ? a : (JsonElement?)null;

        return name switch
        {
            "daily_briefing" => HandleDailyBriefingPrompt(args),
            "research_mode" => HandleResearchModePrompt(args),
            "clean_up_suggestions" => HandleCleanUpPrompt(),
            _ => throw new NotSupportedException($"Prompt '{name}' is not supported.")
        };
    }

    private object HandleDailyBriefingPrompt(JsonElement? args)
    {
        var limit = args?.TryGetProperty("limit", out var l) == true ? l.GetString() : "5";
        return new
        {
            description = "Daily Briefing",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new
                    {
                        type = "text",
                        text = $"Please fetch my latest {limit} unread bookmarks using search_bookmarks. " +
                               "Then, get the content for each of them using get_article_content. " +
                               "Finally, provide a concise briefing summarizing the key points of each article and why they might be interesting to read today."
                    }
                }
            }
        };
    }

    private object HandleResearchModePrompt(JsonElement? args)
    {
        var topic = args?.GetProperty("topic").GetString() ?? throw new ArgumentException("Topic is required.");
        return new
        {
            description = $"Research Mode: {topic}",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new
                    {
                        type = "text",
                        text = $"I want to research '{topic}' in my Instapaper bookmarks. " +
                               $"First, search for bookmarks related to '{topic}' using search_bookmarks. " +
                               "Then, fetch the content of the relevant articles using get_article_content. " +
                               $"Finally, synthesize a comprehensive overview of what my saved articles say about '{topic}', highlighting different perspectives or key data points."
                    }
                }
            }
        };
    }

    private object HandleCleanUpPrompt()
    {
        return new
        {
            description = "Clean Up Suggestions",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new
                    {
                        type = "text",
                        text = "Analyze my recent bookmarks (unread folder). Look for items that might be outdated, " +
                               "redundant, or appear to be low-value (e.g., temporary lists, very old news). " +
                               "Provide a list of suggestions for articles to archive or delete, and explain why for each."
                    }
                }
            }
        };
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
