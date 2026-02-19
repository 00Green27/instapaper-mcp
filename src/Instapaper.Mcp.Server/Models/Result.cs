namespace Instapaper.Mcp.Server;

/// <summary>
/// Represents a failed operation result.
/// </summary>
/// <param name="Exception">The exception that caused the failure.</param>
/// <param name="ErrorMessage">Optional error message.</param>
public record Failure(Exception Exception, string? ErrorMessage = null);

/// <summary>
/// Represents the result of an operation without a return value.
/// </summary>
public abstract record Result
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new SuccessResult();

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result Failed(Exception exception, string? errorMessage = null) => new FailedResult(new Failure(exception, errorMessage));

    public sealed record SuccessResult : Result;

    public sealed record FailedResult(Failure Failure) : Result;
}

/// <summary>
/// Represents the result of an operation with a return value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the return value.</typeparam>
public abstract record Result<T>
{
    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T> Success(T value) => new SuccessResult(value);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result<T> Failed(Exception exception, string? errorMessage = null) => new FailedResult(new Failure(exception, errorMessage));

    public sealed record SuccessResult(T Value) : Result<T>;

    public sealed record FailedResult(Failure Failure) : Result<T>;
}