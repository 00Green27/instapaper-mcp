using Instapaper.Mcp.Server;

namespace Instapaper.Mcp.Server.Tests;

public class ErrorHandlingTests
{
    [Fact]
    public void InstapaperApiException_WithErrorCode_CanBeConstructed()
    {
        var response = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.Unauthorized
        };
        var error = new Error { ErrorCode = InstapaperErrorCode.InvalidAccessToken, Message = "Auth failed" };

        var ex = InstapaperApiException.FromResponse(response, error);

        Assert.IsType<InstapaperApiException>(ex);
        Assert.Equal(InstapaperErrorCode.InvalidAccessToken, ex.ErrorCode);
        Assert.Equal("Instapaper API error (401): Auth failed", ex.Message);
    }

    [Fact]
    public void InstapaperApiException_FromUnknownError_HasDefaultCode()
    {
        var error = new Error { ErrorCode = InstapaperErrorCode.BookmarkNotFound, Message = "Not found" };
        var response = new HttpResponseMessage();

        var ex = InstapaperApiException.FromResponse(response, error);

        Assert.NotNull(ex);
        Assert.Equal(InstapaperErrorCode.BookmarkNotFound, ex.ErrorCode);
    }

    [Fact]
    public void Failure_ContainsExceptionAndMessage()
    {
        var ex = new TimeoutException("timeout");
        var failure = new Failure(ex, "Custom error");

        Assert.Equal(ex, failure.Exception);
        Assert.Equal("Custom error", failure.ErrorMessage);
    }
}