
using System.Security.Cryptography;
using System.Text;

namespace Instapaper.Mcp.Server;

public sealed class OAuth1SignatureGenerator : IOAuth1SignatureGenerator
{
    private readonly TimeProvider _time;

    public OAuth1SignatureGenerator(TimeProvider time)
    {
        _time = time;
    }

    public string CreateAuthorizationHeader(
        HttpMethod method,
        Uri uri,
        string consumerKey,
        string consumerSecret,
        string? token,
        string? tokenSecret,
        Dictionary<string, string>? parameters)
    {
        var oauthParams = new SortedDictionary<string, string>
        {
            ["oauth_consumer_key"] = consumerKey,
            ["oauth_nonce"] = Guid.NewGuid().ToString("N"),
            ["oauth_signature_method"] = "HMAC-SHA1",
            ["oauth_timestamp"] = _time.GetUtcNow().ToUnixTimeSeconds().ToString(),
            ["oauth_version"] = "1.0"
        };

        if (!string.IsNullOrEmpty(token))
        {
            oauthParams.Add("oauth_token", token);
        }

        if (parameters is not null)
        {
            foreach (var p in parameters) oauthParams[p.Key] = p.Value;
        }

        var baseString = BuildBaseString(method, uri, oauthParams);
        var signature = Sign(baseString, consumerSecret, tokenSecret);

        oauthParams["oauth_signature"] = signature;

        return BuildAuthorizationHeader(oauthParams);
    }

    private static string BuildBaseString(
        HttpMethod method,
        Uri uri,
        SortedDictionary<string, string> parameters)
    {
        var paramString = string.Join("&",
            parameters.Select(p => $"{UrlEncode(p.Key)}={UrlEncode(p.Value)}")
        );

        return string.Join("&",
            method.Method.ToUpperInvariant(),
            UrlEncode(uri.GetLeftPart(UriPartial.Path)),
            UrlEncode(paramString)
        );
    }

    private static string Sign(string baseString, string consumerSecret, string? tokenSecret)
    {
        var key = $"{UrlEncode(consumerSecret)}&{(tokenSecret != null ? UrlEncode(tokenSecret) : "")}";
        using var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(key));
        return Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
    }

    private string BuildAuthorizationHeader(SortedDictionary<string, string> oauthParams)
    {
        var headerParams = oauthParams
            .Where(p => p.Key.StartsWith("oauth_"))
            .Select(p => $"{p.Key}=\"{UrlEncode(p.Value)}\"");

        return $"OAuth {string.Join(", ", headerParams)}";
    }

    private static string UrlEncode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Uri.EscapeDataString(value)
            .Replace("!", "%21")
            .Replace("'", "%27")
            .Replace("(", "%28")
            .Replace(")", "%29")
            .Replace("*", "%2A");
    }
}