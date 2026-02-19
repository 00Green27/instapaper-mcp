using Instapaper.Mcp.Server;

namespace Instapaper.Mcp.Server.Tests;

public class ResultPatternTests
{
    [Fact]
    public void Result_Success_CreatesSuccessResult()
    {
        var result = Result.Success();

        Assert.IsType<Result.SuccessResult>(result);
    }

    [Fact]
    public void Result_Failed_CreatesFailedResult()
    {
        var ex = new InvalidOperationException("test error");
        var result = Result.Failed(ex, "custom message");

        var failed = Assert.IsType<Result.FailedResult>(result);
        Assert.Equal(ex, failed.Failure.Exception);
        Assert.Equal("custom message", failed.Failure.ErrorMessage);
    }

    [Fact]
    public void ResultOfT_Success_CreatesSuccessResultWithValue()
    {
        var result = Result<string>.Success("test-value");

        var success = Assert.IsType<Result<string>.SuccessResult>(result);
        Assert.Equal("test-value", success.Value);
    }

    [Fact]
    public void ResultOfT_Failed_CreatesFailedResult()
    {
        var ex = new ArgumentException("arg error");
        var result = Result<int>.Failed(ex);

        var failed = Assert.IsType<Result<int>.FailedResult>(result);
        Assert.Equal(ex, failed.Failure.Exception);
        Assert.Null(failed.Failure.ErrorMessage);
    }

    [Fact]
    public void ResultOfT_Failed_WithMessage_IncludesErrorMessage()
    {
        var ex = new TimeoutException();
        var result = Result<bool>.Failed(ex, "timeout occurred");

        var failed = Assert.IsType<Result<bool>.FailedResult>(result);
        Assert.Equal("timeout occurred", failed.Failure.ErrorMessage);
    }
}