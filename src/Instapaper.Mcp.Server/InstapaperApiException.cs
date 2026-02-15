using System.Net;

namespace Instapaper.Mcp.Server;

/// <summary>
/// Exception thrown when the Instapaper API returns an error response.
/// </summary>
public sealed class InstapaperApiException : Exception
{
    /// <summary>
    /// Gets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets or sets the Instapaper-specific error code.
    /// </summary>
    public InstapaperErrorCode ErrorCode { get; set; }

    private InstapaperApiException(
        string message,
        HttpStatusCode statusCode, InstapaperErrorCode errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates an InstapaperApiException from an HTTP response and error information.
    /// </summary>
    /// <param name="response">The HTTP response message.</param>
    /// <param name="error">The error information from the API response.</param>
    /// <returns>A new InstapaperApiException instance.</returns>
    public static InstapaperApiException FromResponse(HttpResponseMessage response, Error? error)
    {
        return new InstapaperApiException(
            $"Instapaper API error ({(int)response.StatusCode}): {error?.Message ?? "Something went wrong."}",
            response.StatusCode, error?.ErrorCode ?? InstapaperErrorCode.Unknown);
    }
}