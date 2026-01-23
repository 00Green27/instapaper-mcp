using System.Text.Json.Serialization;

namespace InstapaperMcp.Api.Models;

public record McpRequest(
    [property: JsonPropertyName("jsonrpc")]
    string JsonRpc,
    [property: JsonPropertyName("id")] object? Id,
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] object? Params);

public record McpResponse(
    [property: JsonPropertyName("jsonrpc")]
    string JsonRpc,
    [property: JsonPropertyName("id")] object Id,
    [property: JsonPropertyName("result")] object? Result,
    [property: JsonPropertyName("error")] McpError? Error);

public record McpError(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("message")]
    string Message,
    [property: JsonPropertyName("data")] object? Data);
