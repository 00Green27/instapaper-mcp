
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

    [McpServerResource(UriTemplate = "instapaper://bookmarks/unread", Name = "Unread articles", MimeType = "application/json")]
    [Description("List of unread bookmarks with metadata")]
    public async Task<IReadOnlyCollection<Bookmark>> GetBookmarksAsync(CancellationToken cancellationToken) =>
    await _instapaperClient.SearchBookmarksAsync(null, null, null, cancellationToken);
}