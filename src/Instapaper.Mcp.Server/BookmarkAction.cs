namespace Instapaper.Mcp.Server;

/// <summary>
/// Represents actions that can be applied to bookmarks.
/// </summary>
public enum BookmarkAction
{
    /// <summary>
    /// Archive the bookmark.
    /// </summary>
    Archive,

    /// <summary>
    /// Restore the bookmark from archive.
    /// </summary>
    Unarchive,

    /// <summary>
    /// Delete the bookmark.
    /// </summary>
    Delete,

    /// <summary>
    /// Star or favorite the bookmark.
    /// </summary>
    Mark,

    /// <summary>
    /// Add or remove from favorites.
    /// </summary>
    Unmark
}