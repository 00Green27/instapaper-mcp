using System.Net;

namespace Instapaper.Mcp.Server;

public sealed class InstapaperApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public InstapaperErrorCode ErrorCode { get; set; }

    private InstapaperApiException(
        string message,
        HttpStatusCode statusCode, InstapaperErrorCode errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public static InstapaperApiException FromResponse(HttpResponseMessage response, Error? error)
    {
        return new InstapaperApiException(
            $"Instapaper API error ({(int)response.StatusCode}): {error?.Message ?? "Something went wrong."}",
            response.StatusCode, error?.ErrorCode ?? InstapaperErrorCode.Unknown);
    }
}