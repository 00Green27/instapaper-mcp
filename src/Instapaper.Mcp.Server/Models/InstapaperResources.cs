
using System.ComponentModel;

using Instapaper.Mcp.Server;

using ModelContextProtocol.Server;

/// <summary>
/// MCP resources that provide read-only access to Instapaper bookmark collections.
/// </summary>
[McpServerResourceType]
public sealed class InstapaperResources
{
    private readonly IInstapaperClient _instapaperClient;

    /// <summary>
    /// Initializes a new instance of the InstapaperResources class.
    /// </summary>
    /// <param name="instapaperClient">The Instapaper API client.</param>
    public InstapaperResources(IInstapaperClient instapaperClient)
    {
        _instapaperClient = instapaperClient;
    }

    /// <summary>
    /// Gets the list of unread bookmarks.
    /// </summary>
    [McpServerResource(UriTemplate = "instapaper://bookmarks/unread", Name = "Unread bookmarks", MimeType = "application/json")]
    [Description("List of unread bookmarks.")]
    public async Task<IReadOnlyCollection<Bookmark>> GetUnreadBookmarksAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.ListBookmarksAsync(null, null, null, cancellationToken);

    /// <summary>
    /// Gets the list of archived bookmarks.
    /// </summary>
    [McpServerResource(UriTemplate = "instapaper://bookmarks/archive", Name = "Archived bookmarks", MimeType = "application/json")]
    [Description("List of archived bookmarks.")]
    public async Task<IReadOnlyCollection<Bookmark>> GetArchivedBookmarksAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.ListBookmarksAsync(null, "archive", null, cancellationToken);

    /// <summary>
    /// Gets the list of starred bookmarks.
    /// </summary>
    [McpServerResource(UriTemplate = "instapaper://bookmarks/starred", Name = "Starred bookmarks", MimeType = "application/json")]
    [Description("List of starred bookmarks.")]
    public async Task<IReadOnlyCollection<Bookmark>> GetStarredBookmarksAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.ListBookmarksAsync(null, "starred", null, cancellationToken);

    /// <summary>
    /// Gets the list of folders.
    /// </summary>
    [McpServerResource(UriTemplate = "instapaper://folders", Name = "Folders", MimeType = "application/json")]
    [Description("List of all user folders.")]
    public async Task<IReadOnlyCollection<Folder>> GetFoldersAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.ListFoldersAsync(cancellationToken);

    /// <summary>
    /// Gets the bookmark content.
    /// </summary>
    [McpServerResource(UriTemplate = "instapaper://bookmark/{id}", Name = "Bookmark content", MimeType = "application/json")]
    [Description("Full text and metadata for specific bookmark.")]
    public async Task<string> GetBookmarkContentAsync(long id, CancellationToken cancellationToken) =>
    await _instapaperClient.GetBookmarkContentAsync(id, cancellationToken);
}