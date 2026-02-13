namespace Instapaper.Mcp.Server;

public interface IInstapaperClient
{
  Task<IReadOnlyList<Bookmark>> SearchBookmarksAsync(
    string? query,
    long? folderId,
    int? limit,
    CancellationToken ct = default);

  public Task<Bookmark> AddBookmarkAsync(
    string url,
    string? title,
    string? description,
    int? folder_id,
    CancellationToken ct = default);

  Task<IReadOnlyList<Folder>> ListFoldersAsync(CancellationToken ct = default);
}