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
    [Description("archive the article while adding it")]
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

  [McpServerTool(Name = "create_folder")]
  [Description("Creates an organizational folder.")]
  public async Task<Folder> CreateFolderAsync(
    [Description("The title of the folder.")]
    string title,
    CancellationToken cancellationToken)
  {
    try
    {
      return await _instapaperClient.CreateFolderAsync(title, cancellationToken);
    }
    catch (InstapaperApiException ex) when (ex.ErrorCode == InstapaperErrorCode.FolderAlreadyExists)
    {
      var folder = await _instapaperClient.SearchFolderAsync(title, cancellationToken);

      if (folder is null)
      {
        throw new InvalidOperationException("Something went wrong.");
      }
      return folder;
    }
  }

  [McpServerTool(Name = "list_folders")]
  [Description("List folders.")]
  public async Task<IReadOnlyCollection<Folder>> ListFolderAsync(
    CancellationToken cancellationToken) =>
    await _instapaperClient.ListFoldersAsync(cancellationToken);
}