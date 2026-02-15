using System.Text.Json.Serialization;

namespace Instapaper.Mcp.Server;

/// <summary>
/// Base class for all Instapaper API response items.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Meta), "meta")]
[JsonDerivedType(typeof(User), "user")]
[JsonDerivedType(typeof(Bookmark), "bookmark")]
[JsonDerivedType(typeof(Folder), "folder")]
[JsonDerivedType(typeof(Highlight), "highlight")]
[JsonDerivedType(typeof(Error), "error")]
public abstract record InstapaperItem
{
}

/// <summary>
/// Represents metadata in an API response.
/// </summary>
public sealed record Meta : InstapaperItem
{
}

/// <summary>
/// Represents a user account.
/// </summary>
public sealed record User : InstapaperItem
{
    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; init; }

    /// <summary>
    /// Gets the username.
    /// </summary>
    [JsonPropertyName("username")]
    public required string Username { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user has an active subscription.
    /// </summary>
    [JsonPropertyName("subscription_is_active")]
    [JsonConverter(typeof(BoolJsonConverter))]
    public bool SubscriptionIsActive { get; init; }
}

/// <summary>
/// Represents a saved bookmark or article.
/// </summary>
public sealed record Bookmark : InstapaperItem
{
    /// <summary>
    /// Gets the unique identifier for the bookmark.
    /// </summary>
    [JsonPropertyName("bookmark_id")]
    public long BookmarkId { get; init; }

    /// <summary>
    /// Gets the URL of the bookmarked article.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>
    /// Gets the title of the bookmarked article.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>
    /// Gets the folder ID where the bookmark is stored.
    /// </summary>
    [JsonPropertyName("folder_id")]
    public long FolderId { get; init; }

    /// <summary>
    /// Gets the collection of tags associated with the bookmark.
    /// </summary>
    IReadOnlyCollection<Tag> Tags { get; init; } = new List<Tag>();
}

/// <summary>
/// Represents a tag that can be applied to bookmarks.
/// </summary>
public sealed record Tag
{
    /// <summary>
    /// Gets the unique identifier for the tag.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

/// <summary>
/// Represents a folder for organizing bookmarks.
/// </summary>
public sealed record Folder : InstapaperItem
{
    /// <summary>
    /// Gets the unique identifier for the folder.
    /// </summary>
    [JsonPropertyName("folder_id")]
    public long FolderId { get; init; }

    /// <summary>
    /// Gets the title of the folder.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }
}

/// <summary>
/// Represents a highlight or annotation on a bookmarked article.
/// </summary>
public sealed record Highlight : InstapaperItem
{
    /// <summary>
    /// Gets the unique identifier for the highlight.
    /// </summary>
    [JsonPropertyName("highlight_id")]
    public long HighlightId { get; init; }

    /// <summary>
    /// Gets the highlighted text.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }
}

/// <summary>
/// Represents an error response from the Instapaper API.
/// </summary>
public sealed record Error : InstapaperItem
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    [JsonPropertyName("error_code")]
    public InstapaperErrorCode ErrorCode { get; init; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }
}

/// <summary>
/// JSON serialization context for Instapaper API models.
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(List<InstapaperItem>))]
[JsonSerializable(typeof(Meta))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Bookmark))]
[JsonSerializable(typeof(Tag))]
[JsonSerializable(typeof(Folder))]
[JsonSerializable(typeof(Highlight))]
[JsonSerializable(typeof(Error))]
public sealed partial class InstapaperJsonContext : JsonSerializerContext;
