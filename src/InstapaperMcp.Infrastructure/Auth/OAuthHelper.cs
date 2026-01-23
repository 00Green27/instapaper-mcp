using System.Security.Cryptography;
using System.Text;

namespace InstapaperMcp.Infrastructure.Auth;

public static class OAuthHelper
{
    public static string CreateAuthorizationHeader(
        string method,
        string url,
        string consumerKey,
        string consumerSecret,
        string? token = null,
        string? tokenSecret = null,
        IDictionary<string, string>? extraParams = null)
    {
        var oauthParams = new Dictionary<string, string>
        {
            { "oauth_consumer_key", consumerKey },
            { "oauth_nonce", Guid.NewGuid().ToString("N") },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            { "oauth_version", "1.0" }
        };

        if (!string.IsNullOrEmpty(token))
        {
            oauthParams.Add("oauth_token", token);
        }

        var allParams = new Dictionary<string, string>(oauthParams);
        if (extraParams != null)
        {
            foreach (var kvp in extraParams) allParams[kvp.Key] = kvp.Value;
        }

        var sortedParams = allParams.OrderBy(p => p.Key)
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}");
        var paramString = string.Join("&", sortedParams);

        var baseString = $"{method.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";
        var signingKey =
            $"{Uri.EscapeDataString(consumerSecret)}&{(tokenSecret != null ? Uri.EscapeDataString(tokenSecret) : "")}";

        using var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(signingKey));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString)));

        oauthParams.Add("oauth_signature", signature);

        var headerParams = oauthParams.OrderBy(p => p.Key)
            .Select(p => $"{Uri.EscapeDataString(p.Key)}=\"{Uri.EscapeDataString(p.Value)}\"");

        return "OAuth " + string.Join(", ", headerParams);
    }
}
