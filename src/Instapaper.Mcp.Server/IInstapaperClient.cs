namespace Instapaper.Mcp.Server;

public interface IInstapaperClient
{
  Task<IReadOnlyList<Bookmark>> SearchBookmarksAsync(
    string? query,
    long? folderId,
    int? limit,
    CancellationToken ct = default);

  Task<IReadOnlyList<Folder>> ListFoldersAsync(CancellationToken ct = default);
}