namespace Instapaper.Mcp.Server;

public interface IOAuth1SignatureGenerator
{
    string CreateAuthorizationHeader(
        HttpMethod method,
        Uri uri,
        string consumerKey,
        string consumerSecret,
        string? token,
        string? tokenSecret,
        Dictionary<string, string>? parameters);
}