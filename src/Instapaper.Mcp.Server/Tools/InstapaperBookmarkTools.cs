using System.ComponentModel;
using Instapaper.Mcp.Server;
using ModelContextProtocol.Server;

/// <summary>
/// MCP tools for managing bookmarks in Instapaper.
/// </summary>
[McpServerToolType]
public sealed class InstapaperBookmarkTools
{
  private readonly IInstapaperClient _instapaperClient;

  /// <summary>
  /// Initializes a new instance of the InstapaperBookmarkTools class.
  /// </summary>
  /// <param name="instapaperClient">The Instapaper API client.</param>
  public InstapaperBookmarkTools(IInstapaperClient instapaperClient)
  {
    _instapaperClient = instapaperClient;
  }

  /// <summary>
  /// Lists or searches bookmarks.
  /// </summary>
  [McpServerTool(Name = "list_bookmarks")]
  [Description("List or search bookmarks. Defaults to unread folder.")]
  public async Task<IReadOnlyCollection<Bookmark>> ListBookmarksAsync(
    [Description("Optional search query.")]
    string? query,
    [Description("Optional folder ID to search in. Possible values are unread (default), starred, archive, or a folder ID")]
    string? folderId,
    [Description("Maximum number of items to return (default 100).")]
    string? limit,
    CancellationToken cancellationToken)
  {
    int.TryParse(limit, out var parseLimit);
    return await _instapaperClient.ListBookmarksAsync(query, folderId, parseLimit, cancellationToken);
  }

  /// <summary>
  /// Adds a new bookmark or note.
  /// </summary>
  [McpServerTool(Name = "add_bookmark")]
  [Description("Adds a new bookmark or note.")]
  public async Task<Bookmark> AddBookmarkAsync(
  [Description("The URL of the bookmark or note.")]
    string? url,
  [Description("Optional title.")]
    string? title,
  [Description("Optional description.")]
    string? description,
  [Description("Optional folder ID to add to.")]
    int? folderId,
  [Description("The full HTML content of the page.")]
    string? content,
  [Description("Optional controls whether redirects are resolved before saving the URL.")]
    bool resolveFinalUrl,
  [Description("Archive the bookmark while adding it.")]
    bool archiveOnAdd,
  [Description("Optional. List of tags. Tags will be created if they do not already exist.")]
    List<string> tags,
  CancellationToken cancellationToken) =>
await _instapaperClient.AddBookmarkAsync(url, title, description, folderId, content, resolveFinalUrl, archiveOnAdd, tags, cancellationToken);

  /// <summary>
  /// Moves multiple bookmarks to a different folder.
  /// </summary>
  [McpServerTool(Name = "move_bookmarks")]
  [Description("Move bookmarks to a different folder.")]
  public async Task<IReadOnlyCollection<Bookmark>> MoveBookmarksAsync(
    [Description("List of bookmark IDs to move.")]
    List<long> bookmarkIds,
    [Description("The target folder ID.")]
    long folderId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.MoveBookmarksAsync(bookmarkIds, folderId, cancellationToken);

  /// <summary>
  /// Moves a single bookmark to a different folder.
  /// </summary>
  [McpServerTool(Name = "move_bookmark")]
  [Description("Moves a bookmark to a different folder.")]
  public async Task<Bookmark> MoveBookmarkAsync(
    [Description("The bookmark ID to move.")]
    long bookmarkId,
    [Description("The target folder ID.")]
    long folderId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.MoveBookmarkAsync(bookmarkId, folderId, cancellationToken);

  /// <summary>
  /// Archives a single bookmark.
  /// </summary>
  [McpServerTool(Name = "archive_bookmark")]
  [Description("Moves a bookmark to archive.")]
  public async Task<Bookmark> ArchiveBookmarkAsync(
    [Description("The bookmark ID to archive.")]
    long bookmarkId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkId, BookmarkAction.Archive, cancellationToken);

  /// <summary>
  /// Archives multiple bookmarks.
  /// </summary>
  [McpServerTool(Name = "archive_bookmarks")]
  [Description("Move bookmarks to archive.")]
  public async Task<IReadOnlyCollection<Bookmark>> ArchiveBookmarksAsync(
    [Description("List of bookmark IDs to archive.")]
    List<long> bookmarkIds,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkIds, BookmarkAction.Archive, cancellationToken);

  /// <summary>
  /// Restores a bookmark from archive.
  /// </summary>
  [McpServerTool(Name = "unarchive_bookmark")]
  [Description("Restore bookmark from archive.")]
  public async Task<Bookmark> UnarchiveBookmarkAsync(
    [Description("The bookmark ID to restore.")]
    long bookmarkId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkId, BookmarkAction.Unarchive, cancellationToken);

  /// <summary>
  /// Marks multiple bookmarks as important (stars it).
  /// </summary>
  [McpServerTool(Name = "mark_bookmarks")]
  [Description("Mark bookmarks as important.")]
  public async Task<IReadOnlyCollection<Bookmark>> MarkBookmarksAsync(
    [Description("List of bookmark IDs to mark as important.")]
    List<long> bookmarkIds,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkIds, BookmarkAction.Mark, cancellationToken);

  /// <summary>
  /// Marks a bookmark as important (stars it).
  /// </summary>
  [McpServerTool(Name = "mark_bookmark")]
  [Description("Mark bookmark as important.")]
  public async Task<Bookmark> MarkBookmarkAsync(
    [Description("The bookmark ID to mark as important.")]
    long bookmarkId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkId, BookmarkAction.Mark, cancellationToken);

  /// <summary>
  /// Unmarks multiple bookmarks as important (unstars it).
  /// </summary>
  [McpServerTool(Name = "unmark_bookmarks")]
  [Description("Unmark bookmarks as important.")]
  public async Task<IReadOnlyCollection<Bookmark>> UnmarkBookmarksAsync(
    [Description("List of bookmark IDs to unmark as important.")]
    List<long> bookmarkIds,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkIds, BookmarkAction.Unmark, cancellationToken);

  /// <summary>
  /// Unmarks a bookmark as important (unstars it).
  /// </summary>
  [McpServerTool(Name = "unmark_bookmark")]
  [Description("Unmarks a bookmark as important.")]
  public async Task<Bookmark> UnmarkBookmarkAsync(
    [Description("The bookmark ID to unmark.")]
    long bookmarkId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkId, BookmarkAction.Unmark, cancellationToken);
}