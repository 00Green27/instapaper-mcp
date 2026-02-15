namespace Instapaper.Mcp.Server;

/// <summary>
/// Generates OAuth 1.0a authorization signatures for API requests.
/// </summary>
public interface IOAuth1SignatureGenerator
{
    /// <summary>
    /// Creates an OAuth 1.0a Authorization header value.
    /// </summary>
    /// <param name="method">The HTTP method (GET, POST, etc.).</param>
    /// <param name="uri">The request URI.</param>
    /// <param name="consumerKey">The OAuth consumer key.</param>
    /// <param name="consumerSecret">The OAuth consumer secret.</param>
    /// <param name="token">The OAuth access token, or null for 2-legged OAuth.</param>
    /// <param name="tokenSecret">The OAuth token secret, or null for 2-legged OAuth.</param>
    /// <param name="parameters">Request parameters for signature calculation.</param>
    /// <returns>The OAuth Authorization header value.</returns>
    string CreateAuthorizationHeader(
        HttpMethod method,
        Uri uri,
        string consumerKey,
        string consumerSecret,
        string? token,
        string? tokenSecret,
        Dictionary<string, string>? parameters);
}