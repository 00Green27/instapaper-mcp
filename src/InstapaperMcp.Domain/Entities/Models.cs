namespace InstapaperMcp.Domain.Entities;

public sealed record Bookmark(
    long Id,
    string Url,
    string Title,
    string Description,
    string Content,
    long FolderId,
    bool IsStarred,
    DateTime UpdatedAt);

public sealed record Folder(
    long Id,
    string Title,
    int Position,
    string Slug);

public sealed record Highlight(
    long Id,
    long BookmarkId,
    string Text,
    string Note,
    DateTime CreatedAt);
