using InstapaperMcp.Domain.Entities;
using InstapaperMcp.Domain.Common;

namespace InstapaperMcp.Application.Interfaces;

public interface IInstapaperService
{
    Task<Result<IReadOnlyList<Bookmark>>> SearchBookmarksAsync(long? folderId, string? query, int limit, CancellationToken ct);
    Task<Result<IReadOnlyList<Bookmark>>> GetArticlesContentAsync(IReadOnlyList<long> bookmarkIds, CancellationToken ct);
    Task<Result<Bookmark>> AddBookmarkAsync(string? url, string? content, string? title, string? description, long? folderId, CancellationToken ct);
    Task<Result> ManageBookmarksAsync(IReadOnlyList<long> bookmarkIds, BookmarkAction action, CancellationToken ct);
    Task<Result> MoveBookmarksAsync(IReadOnlyList<long> bookmarkIds, long folderId, CancellationToken ct);

    Task<Result<IReadOnlyList<Folder>>> ListFoldersAsync(CancellationToken ct);
    Task<Result<Folder>> CreateFolderAsync(string title, CancellationToken ct);
    Task<Result> DeleteFolderAsync(long folderId, CancellationToken ct);

    Task<Result<IReadOnlyList<Highlight>>> ListHighlightsAsync(long bookmarkId, CancellationToken ct);
    Task<Result<Highlight>> AddHighlightAsync(long bookmarkId, string text, string? note, CancellationToken ct);
    Task<Result> DeleteHighlightAsync(long highlightId, CancellationToken ct);
}

public enum BookmarkAction
{
    Archive,
    Unarchive,
    Delete,
    Star,
    Unstar
}
