using System.Text.Json.Serialization;

namespace Instapaper.Mcp.Server;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Meta), "meta")]
[JsonDerivedType(typeof(User), "user")]
[JsonDerivedType(typeof(Bookmark), "bookmark")]
[JsonDerivedType(typeof(Folder), "folder")]
[JsonDerivedType(typeof(Highlight), "highlight")]
public abstract record InstapaperItem
{
}

public sealed record Meta : InstapaperItem
{
}

public sealed record User : InstapaperItem
{
    [JsonPropertyName("user_id")]
    public long UserId { get; init; }

    [JsonPropertyName("username")]
    public required string Username { get; init; }

    [JsonPropertyName("subscription_is_active")]
    [JsonConverter(typeof(BoolJsonConverter))]
    public bool SubscriptionIsActive { get; init; }
}

public sealed record Bookmark : InstapaperItem
{
    [JsonPropertyName("bookmark_id")]
    public long BookmarkId { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }

    [JsonPropertyName("folder_id")]
    public long FolderId { get; init; }
}

public sealed record Folder : InstapaperItem
{
    [JsonPropertyName("folder_id")]
    public long FolderId { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }
}

public sealed record Highlight : InstapaperItem
{
    [JsonPropertyName("highlight_id")]
    public long HighlightId { get; init; }

    [JsonPropertyName("text")]
    public required string Text { get; init; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(List<InstapaperItem>))]
[JsonSerializable(typeof(Meta))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Bookmark))]
[JsonSerializable(typeof(Folder))]
[JsonSerializable(typeof(Highlight))]
public sealed partial class InstapaperJsonContext : JsonSerializerContext;