namespace Instapaper.Mcp.Server.Configuration;

/// <summary>
/// Configuration options for the Instapaper API client.
/// </summary>
public sealed class InstapaperOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Instapaper";

    /// <summary>
    /// Gets or sets the OAuth consumer key for API access.
    /// </summary>
    public string ConsumerKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth consumer secret for API access.
    /// </summary>
    public string ConsumerSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Instapaper username for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Instapaper password for authentication.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cached OAuth access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the cached OAuth access token secret.
    /// </summary>
    public string? AccessTokenSecret { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items per API request.
    /// Default is 500.
    /// </summary>
    public int MaxApiLimit { get; set; } = 500;

    /// <summary>
    /// Gets or sets the maximum number of pages to fetch when listing bookmarks.
    /// Default is 10.
    /// </summary>
    public int MaxPages { get; set; } = 10;
}