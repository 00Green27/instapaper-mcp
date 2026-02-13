namespace Instapaper.Mcp.Server.Configuration;

public sealed class InstapaperOptions
{
    public const string SectionName = "Instapaper";

    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? AccessTokenSecret { get; set; }
}
