using System.Reflection;
using System.Text.Json;
using InstapaperMcp.Api.Handlers;
using InstapaperMcp.Api.Models;
using InstapaperMcp.Application.Interfaces;
using InstapaperMcp.Infrastructure.Configuration;
using InstapaperMcp.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InstapaperMcp.Api;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Logging refined for MCP: Strictly to Standard Error
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options => { options.LogToStandardErrorThreshold = LogLevel.Trace; });

        // Configuration
        builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true);

        builder.Services.Configure<InstapaperOptions>(
            builder.Configuration.GetSection(InstapaperOptions.SectionName));

        // Core Services
        builder.Services.AddHttpClient<IInstapaperService, InstapaperService>(client =>
            client.BaseAddress = new Uri("https://www.instapaper.com/api/1/"));
        builder.Services.AddScoped<McpHandler>();

        using var host = builder.Build();
        var logger = host.Services.GetRequiredService<ILogger<McpHandler>>();

        // Read from stdin, Write to stdout
        using var stdin = Console.OpenStandardInput();
        using var stdout = Console.OpenStandardOutput();

        logger.LogInformation("Instapaper MCP Server starting...");
        logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);

        var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<InstapaperOptions>>()
            .Value;
        logger.LogInformation("Configuration Check:");
        logger.LogInformation("- ConsumerKey: {IsSet}", !string.IsNullOrEmpty(options.ConsumerKey) ? "SET" : "MISSING");
        logger.LogInformation("- ConsumerSecret: {IsSet}",
            !string.IsNullOrEmpty(options.ConsumerSecret) ? "SET" : "MISSING");
        logger.LogInformation("- Username: {IsSet}", !string.IsNullOrEmpty(options.Username) ? "SET" : "MISSING");
        logger.LogInformation("- Password: {IsSet}", !string.IsNullOrEmpty(options.Password) ? "SET" : "MISSING");
        logger.LogInformation("- AccessToken: {IsSet}", !string.IsNullOrEmpty(options.AccessToken) ? "SET" : "MISSING");
        logger.LogInformation("- AccessTokenSecret: {IsSet}",
            !string.IsNullOrEmpty(options.AccessTokenSecret) ? "SET" : "MISSING");

        try
        {
            // Use a buffer-based reader for robust JSON-RPC handling
            var reader = new Utf8JsonStreamReader(stdin);

            while (await reader.ReadAsync())
            {
                var json = reader.CurrentJson;
                if (string.IsNullOrWhiteSpace(json)) continue;

                try
                {
                    var request = JsonSerializer.Deserialize<McpRequest>(json, JsonOptions);
                    if (request == null) continue;

                    // Handle notifications vs requests
                    using var scope = host.Services.CreateScope();
                    var handler = scope.ServiceProvider.GetRequiredService<McpHandler>();

                    try
                    {
                        var result = await handler.HandleAsync(
                            request.Method,
                            (JsonElement?)request.Params,
                            CancellationToken.None);

                        // Notifications have no ID and expect no response
                        if (request.Id != null)
                        {
                            var response = new McpResponse("2.0", request.Id, result, null);
                            var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
                            var responseBytes = System.Text.Encoding.UTF8.GetBytes(jsonResponse + "\n");
                            await stdout.WriteAsync(responseBytes);
                            await stdout.FlushAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing request {Id}: {Method}", request.Id, request.Method);
                        if (request.Id != null)
                        {
                            var errorResponse = new
                            {
                                jsonrpc = "2.0",
                                id = request.Id,
                                error = new
                                {
                                    code = -32603,
                                    message = ex.Message,
                                    data = ex.ToString()
                                }
                            };
                            var jsonResponse = JsonSerializer.Serialize(errorResponse, JsonOptions);
                            var responseBytes = System.Text.Encoding.UTF8.GetBytes(jsonResponse + "\n");
                            await stdout.WriteAsync(responseBytes);
                            await stdout.FlushAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Fatal error parsing JSON: {Json}", json);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Fatal error in MCP server loop");
        }
    }
}

/// <summary>
/// A simple helper to read individual JSON objects from a stream.
/// </summary>
internal class Utf8JsonStreamReader(Stream stream)
{
    private readonly byte[] _buffer = new byte[8192];
    private int _bufferCount = 0;
    public string? CurrentJson { get; private set; }

    public async Task<bool> ReadAsync()
    {
        // This is a simplified balanced-brace tracker for JSON-RPC.
        // For production robustness, a real Utf8JsonReader based parser is better.
        int braceCount = 0;
        bool inString = false;
        bool escaped = false;
        var result = new System.Text.StringBuilder();

        while (true)
        {
            if (_bufferCount == 0)
            {
                _bufferCount = await stream.ReadAsync(_buffer);
                if (_bufferCount == 0) return false;
            }

            for (int i = 0; i < _bufferCount; i++)
            {
                byte b = _buffer[i];
                char c = (char)b;
                result.Append(c);

                if (!inString)
                {
                    if (c == '{') braceCount++;
                    else if (c == '}') braceCount--;
                    else if (c == '"') inString = true;
                }
                else
                {
                    if (escaped) escaped = false;
                    else if (c == '\\') escaped = true;
                    else if (c == '"') inString = false;
                }

                if (braceCount == 0 && result.Length > 0 && !char.IsWhiteSpace(c))
                {
                    // Potentially found balanced object
                    // Check if it's actually an object (starts with {)
                    var possibleJson = result.ToString().Trim();
                    if (possibleJson.StartsWith("{") && possibleJson.EndsWith("}"))
                    {
                        CurrentJson = possibleJson;
                        // Move remaining buffer
                        var remaining = _bufferCount - (i + 1);
                        if (remaining > 0)
                        {
                            Array.Copy(_buffer, i + 1, _buffer, 0, remaining);
                        }

                        _bufferCount = remaining;
                        return true;
                    }
                }
            }

            _bufferCount = 0;
        }
    }
}
