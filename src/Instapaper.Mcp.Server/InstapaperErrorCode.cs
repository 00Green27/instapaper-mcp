using System.ComponentModel;

namespace Instapaper.Mcp.Server;

/// <summary>
/// Known Instapaper API error codes returned in JSON responses:
/// { "type": "error", "error_code": ..., "message": "..." }
/// </summary>
public enum InstapaperErrorCode
{
    Unknown = 0,
    /// <summary>
    /// Invalid OAuth request (malformed signature, missing parameters, etc.).
    /// </summary>
    [Description("Invalid OAuth request")]
    InvalidOAuthRequest = 1100,

    /// <summary>
    /// Invalid consumer key.
    /// </summary>
    [Description("Invalid consumer key")]
    InvalidConsumerKey = 1101,

    /// <summary>
    /// Invalid or expired access token.
    /// </summary>
    [Description("Invalid access token")]
    InvalidAccessToken = 1102,

    /// <summary>
    /// Bookmark not found.
    /// </summary>
    [Description("Bookmark not found")]
    BookmarkNotFound = 1241,

    /// <summary>
    /// Folder not found.
    /// </summary>
    [Description("Folder not found")]
    FolderNotFound = 1242,

    /// <summary>
    /// Invalid folder identifier.
    /// </summary>
    [Description("Invalid folder")]
    InvalidFolder = 1243,

    /// <summary>
    /// Invalid bookmark identifier.
    /// </summary>
    [Description("Invalid bookmark id")]
    InvalidBookmarkId = 1244,

    /// <summary>
    /// Bookmark already starred.
    /// </summary>
    [Description("Bookmark already starred")]
    BookmarkAlreadyStarred = 1245,

    /// <summary>
    /// Bookmark already archived.
    /// </summary>
    [Description("Bookmark already archived")]
    BookmarkAlreadyArchived = 1246,

    /// <summary>
    /// User already has a folder with this title.
    /// </summary>
    [Description("User already has a folder with this title")]
    FolderAlreadyExists = 1251
}