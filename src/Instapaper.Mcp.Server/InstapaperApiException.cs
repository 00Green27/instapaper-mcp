using System.Net;

namespace Instapaper.Mcp.Server;

public sealed class InstapaperApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    private InstapaperApiException(
        string message,
        HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public static InstapaperApiException FromResponse(HttpResponseMessage response, string payload)
    {
        return new InstapaperApiException(
            $"Instapaper API error ({(int)response.StatusCode}): {payload}",
            response.StatusCode);
    }
}