using System.ComponentModel;
using Instapaper.Mcp.Server;
using ModelContextProtocol.Server;

[McpServerToolType]
public sealed class InstapaperBookmarkTools
{
  private readonly IInstapaperClient _instapaperClient;

  public InstapaperBookmarkTools(IInstapaperClient instapaperClient)
  {
    _instapaperClient = instapaperClient;
  }

  [McpServerTool(Name = "list_bookmarks")]
  [Description("List or search bookmarks. Defaults to unread folder.")]
  public async Task<IReadOnlyCollection<Bookmark>> ListBookmarksAsync(
    [Description("Optional search query.")]
    string? query,
    [Description("Optional folder ID to search in. Possible values are unread (default), starred, archive, or a folder ID")]
    string? folderId,
    [Description("Maximum number of items to return (default 100).")]
    int? limit,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ListBookmarksAsync(query, folderId, limit, cancellationToken);

  [McpServerTool(Name = "add_bookmark")]
  [Description("Add a new bookmark or note.")]
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
    CancellationToken cancellationToken) =>
await _instapaperClient.AddBookmarkAsync(url, title, description, folderId, content, resolveFinalUrl, archiveOnAdd, cancellationToken);

  [McpServerTool(Name = "move_bookmarks")]
  [Description("Move bookmarks to a different folder.")]
  public async Task<IReadOnlyCollection<Bookmark>> MoveBookmarksAsync(
    [Description("List of bookmark IDs to move.")]
    IEnumerable<long> bookmarkIds,
    [Description("The target folder ID.")]
    long folderId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.MoveBookmarksAsync(bookmarkIds, folderId, cancellationToken);

  [McpServerTool(Name = "move_bookmark")]
  [Description("Move bookmarks to a different folder.")]
  public async Task<Bookmark> MoveBookmarkAsync(
    [Description("The bookmark ID to move.")]
    long bookmarkId,
    [Description("The target folder ID.")]
    long folderId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.MoveBookmarkAsync(bookmarkId, folderId, cancellationToken);

  [McpServerTool(Name = "archive_bookmark")]
  [Description("Move bookmark to archive.")]
  public async Task<Bookmark> ArchiveBookmarkAsync(
    [Description("The bookmark ID to archive.")]
    long bookmarkId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkId, BookmarkAction.Archive, cancellationToken);

  [McpServerTool(Name = "archive_bookmark")]
  [Description("Move bookmark to archive.")]
  public async Task<IReadOnlyCollection<Bookmark>> ArchiveBookmarksAsync(
    [Description("List of bookmark IDs to archive.")]
    IEnumerable<long> bookmarkIds,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkIds, BookmarkAction.Archive, cancellationToken);

  [McpServerTool(Name = "unarchive_bookmark")]
  [Description("Restore bookmark from archive.")]
  public async Task<Bookmark> UnarchiveBookmarkAsync(
    [Description("The bookmark ID to restore.")]
    long bookmarkId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkId, BookmarkAction.Unarchive, cancellationToken);

  [McpServerTool(Name = "mark_bookmark")]
  [Description("Mark bookmark as important.")]
  public async Task<Bookmark> MarkBookmarkAsync(
    [Description("The bookmark ID to archive.")]
    long bookmarkId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkId, BookmarkAction.Mark, cancellationToken);

  [McpServerTool(Name = "unmark_bookmark")]
  [Description("Unmark bookmark as important.")]
  public async Task<Bookmark> UnmarkBookmarkAsync(
    [Description("The bookmark ID to restore.")]
    long bookmarkId,
    CancellationToken cancellationToken) =>
    await _instapaperClient.ManageBookmarksAsync(bookmarkId, BookmarkAction.Unmark, cancellationToken);
}