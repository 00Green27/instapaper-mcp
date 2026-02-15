namespace Instapaper.Mcp.Server;

/// <summary>
/// Provides access to the Instapaper API for bookmark and folder management.
/// </summary>
public interface IInstapaperClient
{
  /// <summary>
  /// Lists bookmarks with optional filtering and pagination.
  /// </summary>
  /// <param name="query">Optional search query to filter bookmarks by title.</param>
  /// <param name="folderId">Optional folder ID to list bookmarks from a specific folder.</param>
  /// <param name="limit">Maximum number of bookmarks to return. Defaults to 100 if not specified.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A collection of bookmarks matching the criteria.</returns>
  Task<IReadOnlyCollection<Bookmark>> ListBookmarksAsync(string? query, string? folderId, int? limit, CancellationToken ct = default);

  /// <summary>
  /// Adds a new bookmark to Instapaper.
  /// </summary>
  /// <param name="url">The URL of the bookmark.</param>
  /// <param name="title">Optional title for the bookmark.</param>
  /// <param name="description">Optional description or notes for the bookmark.</param>
  /// <param name="folderId">Optional folder ID to save the bookmark to.</param>
  /// <param name="content">Optional full HTML content of the page.</param>
  /// <param name="resolveFinalUrl">Whether to resolve redirect URLs before saving.</param>
  /// <param name="archiveOnAdd">Whether to immediately archive the bookmark after adding.</param>
  /// <param name="tags">Optional list of tags to assign to the bookmark.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>The created bookmark.</returns>
  Task<Bookmark> AddBookmarkAsync(string? url = null, string? title = null, string? description = null, long? folderId = null, string? content = null, bool resolveFinalUrl = true, bool archiveOnAdd = false, List<string>? tags = null, CancellationToken ct = default);

  /// <summary>
  /// Retrieves the full HTML content of a bookmark.
  /// </summary>
  /// <param name="bookmarkId">The ID of the bookmark.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>The HTML content of the bookmark.</returns>
  Task<string> GetBookmarkContentAsync(long bookmarkId, CancellationToken ct = default);

  /// <summary>
  /// Retrieves the HTML content for multiple bookmarks.
  /// </summary>
  /// <param name="bookmarkIds">The IDs of the bookmarks.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A dictionary mapping bookmark IDs to their HTML content.</returns>
  Task<IReadOnlyDictionary<long, string>> GetBookmarkContentsAsync(IEnumerable<long> bookmarkIds, CancellationToken ct = default);

  /// <summary>
  /// Applies an action to a single bookmark (archive, star, delete, etc.).
  /// </summary>
  /// <param name="bookmarkId">The ID of the bookmark.</param>
  /// <param name="action">The action to apply.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>The updated bookmark.</returns>
  Task<Bookmark> ManageBookmarksAsync(long bookmarkId, BookmarkAction action, CancellationToken ct = default);

  /// <summary>
  /// Applies an action to multiple bookmarks.
  /// </summary>
  /// <param name="bookmarkIds">The IDs of the bookmarks.</param>
  /// <param name="action">The action to apply to all bookmarks.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>The updated bookmarks.</returns>
  Task<IReadOnlyCollection<Bookmark>> ManageBookmarksAsync(IEnumerable<long> bookmarkIds, BookmarkAction action, CancellationToken ct = default);

  /// <summary>
  /// Moves a bookmark to a different folder.
  /// </summary>
  /// <param name="bookmarkId">The ID of the bookmark.</param>
  /// <param name="folderId">The ID of the target folder.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>The moved bookmark.</returns>
  Task<Bookmark> MoveBookmarkAsync(long bookmarkId, long folderId, CancellationToken ct = default);

  /// <summary>
  /// Moves multiple bookmarks to a different folder.
  /// </summary>
  /// <param name="bookmarkIds">The IDs of the bookmarks.</param>
  /// <param name="folderId">The ID of the target folder.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>The moved bookmarks.</returns>
  Task<IReadOnlyCollection<Bookmark>> MoveBookmarksAsync(IEnumerable<long> bookmarkIds, long folderId, CancellationToken ct = default);

  /// <summary>
  /// Searches for a folder by title.
  /// </summary>
  /// <param name="title">The title of the folder to search for.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>The folder if found; otherwise, null.</returns>
  Task<Folder?> SearchFolderAsync(string title, CancellationToken ct = default);

  /// <summary>
  /// Lists all folders.
  /// </summary>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>A collection of all folders.</returns>
  Task<IReadOnlyCollection<Folder>> ListFoldersAsync(CancellationToken ct = default);

  /// <summary>
  /// Creates a new folder.
  /// </summary>
  /// <param name="title">The title for the new folder.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>The created folder.</returns>
  Task<Folder> CreateFolderAsync(string title, CancellationToken ct = default);

  /// <summary>
  /// Deletes a folder.
  /// </summary>
  /// <param name="folderId">The ID of the folder to delete.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>True if the folder was deleted; otherwise, false.</returns>
  Task<bool> DeleteFolderAsync(long folderId, CancellationToken ct = default);

  /// <summary>
  /// Reorders folders.
  /// </summary>
  /// <param name="folderOrders">An array of folder IDs and their new positions.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A collection of all folders in their new order.</returns>
  Task<IReadOnlyCollection<Folder>> ReorderFoldersAsync((long FolderId, int Position)[] folderOrders, CancellationToken cancellationToken);
}
