namespace Instapaper.Mcp.Server;

public interface IInstapaperClient
{
  Task<IReadOnlyCollection<Bookmark>> ListBookmarksAsync(string? query, string? folderId, int? limit, CancellationToken ct = default);
  Task<Bookmark> AddBookmarkAsync(string? url = null, string? title = null, string? description = null, long? folderId = null, string? content = null, bool resolveFinalUrl = true, bool archiveOnAdd = false, List<string>? tags = null, CancellationToken ct = default);
  Task<string> GetBookmarkContentAsync(long bookmarkId, CancellationToken ct = default);
  Task<IReadOnlyDictionary<long, string>> GetBookmarkContentsAsync(IEnumerable<long> bookmarkIds, CancellationToken ct = default);
  Task<Bookmark> ManageBookmarksAsync(long bookmarkId, BookmarkAction action, CancellationToken ct = default);
  Task<IReadOnlyCollection<Bookmark>> ManageBookmarksAsync(IEnumerable<long> bookmarkIds, BookmarkAction action, CancellationToken ct = default);
  Task<Bookmark> MoveBookmarkAsync(long bookmarkId, long folderId, CancellationToken ct = default);
  Task<IReadOnlyCollection<Bookmark>> MoveBookmarksAsync(IEnumerable<long> bookmarkIds, long folderId, CancellationToken ct = default);
  Task<Folder?> SearchFolderAsync(string title, CancellationToken ct = default);
  Task<IReadOnlyCollection<Folder>> ListFoldersAsync(CancellationToken ct = default);
  Task<Folder> CreateFolderAsync(string title, CancellationToken ct = default);
  Task<bool> DeleteFolderAsync(long folderId, CancellationToken ct = default);
  Task<IReadOnlyCollection<Folder>> ReorderFoldersAsync((long folderId, int position)[] folderOrders, CancellationToken cancellationToken);
}
