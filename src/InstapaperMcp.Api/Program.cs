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

        // Logging refined for MCP
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            // MCP servers must log to stderr to avoid polluting JSON-RPC on stdout
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        // Configuration
        builder.Services.Configure<InstapaperOptions>(
            builder.Configuration.GetSection(InstapaperOptions.SectionName));

        // Core Services
        builder.Services.AddHttpClient<IInstapaperService, InstapaperService>();
        builder.Services.AddScoped<McpHandler>();

        using var host = builder.Build();

        var logger = host.Services.GetRequiredService<ILogger<McpHandler>>();
        logger.LogInformation("Instapaper MCP Server started.");

        using var reader = new StreamReader(Console.OpenStandardInput());

        while (await reader.ReadLineAsync() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(line, JsonOptions);
                if (request == null) continue;

                using var scope = host.Services.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<McpHandler>();

                McpResponse response;
                try
                {
                    var result = await handler.HandleAsync(
                        request.Method,
                        (JsonElement?)request.Params,
                        CancellationToken.None);

                    response = new McpResponse("2.0", request.Id, result, null);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling method '{Method}'", request.Method);
                    response = new McpResponse("2.0", request.Id, null, new McpError(-32603, ex.Message, null));
                }

                var jsonResponse = JsonSerializer.Serialize(response, JsonOptions);
                await Console.Out.WriteLineAsync(jsonResponse);
                await Console.Out.FlushAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error processing request line.");
            }
        }
    }
}
