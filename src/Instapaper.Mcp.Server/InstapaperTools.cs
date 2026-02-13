using System.ComponentModel;
using Instapaper.Mcp.Server;
using ModelContextProtocol.Server;

[McpServerToolType]
public sealed class InstapaperTools
{
  private readonly IInstapaperClient _instapaperClient;

  public InstapaperTools(IInstapaperClient instapaperClient)
  {
    _instapaperClient = instapaperClient;
  }

  [McpServerTool(Name = "search_bookmarks")]
  [Description("List or search bookmarks. Defaults to unread folder.")]
  public async Task<IReadOnlyList<Bookmark>> SearchBookmarksAsync(
    [Description("Optional search query.")]
    string? query,
    [Description("Optional folder ID to search in.")]
    long? folderId,
    [Description("Maximum number of items to return (default 100).")]
    int? limit,
    CancellationToken cancellationToken) =>
    await _instapaperClient.SearchBookmarksAsync(query, folderId, limit, cancellationToken);
}