
using System.ComponentModel;
using Instapaper.Mcp.Server;
using ModelContextProtocol.Server;

[McpServerResourceType]
public sealed class InstapaperResources
{
    private readonly IInstapaperClient _instapaperClient;

    public InstapaperResources(IInstapaperClient instapaperClient)
    {
        _instapaperClient = instapaperClient;
    }

    [McpServerResource(UriTemplate = "instapaper://bookmarks/unread", Name = "Unread bookmarks", MimeType = "application/json")]
    [Description("List of unread bookmarks.")]
    public async Task<IReadOnlyCollection<Bookmark>> GetUnreadBookmarksAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.ListBookmarksAsync(null, null, null, cancellationToken);

    [McpServerResource(UriTemplate = "instapaper://bookmarks/archive", Name = "Archived bookmarks", MimeType = "application/json")]
    [Description("List of archived bookmarks.")]
    public async Task<IReadOnlyCollection<Bookmark>> GetArchivedBookmarksAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.ListBookmarksAsync(null, "archive", null, cancellationToken);

    [McpServerResource(UriTemplate = "instapaper://bookmarks/starred", Name = "Starred bookmarks", MimeType = "application/json")]
    [Description("List of starred bookmarks.")]
    public async Task<IReadOnlyCollection<Bookmark>> GetStarredBookmarksAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.ListBookmarksAsync(null, "starred", null, cancellationToken);

    [McpServerResource(UriTemplate = "instapaper://folders", Name = "Folders", MimeType = "application/json")]
    [Description("List of all user folders.")]
    public async Task<IReadOnlyCollection<Folder>> GetFoldersAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.ListFoldersAsync(cancellationToken);

    [McpServerResource(UriTemplate = "instapaper://bookmark/{id}", Name = "Bookmark content", MimeType = "application/json")]
    [Description("Full text and metadata for specific bookmark.")]
    public async Task<string> GetBookmarkContentAsync(long id, CancellationToken cancellationToken) =>
    await _instapaperClient.GetBookmarkContentAsync(id, cancellationToken);
}