using System.Text.Json;
using InstapaperMcp.Api.Handlers;
using InstapaperMcp.Api.Models;
using InstapaperMcp.Application.Interfaces;
using InstapaperMcp.Infrastructure.Configuration;
using InstapaperMcp.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InstapaperMcp.Api;

internal class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            // Redirect console logging to stderr to not interfere with MCP (JSON-RPC on stdout)
            builder.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
        });

        services.Configure<InstapaperOptions>(configuration.GetSection(InstapaperOptions.SectionName));
        services.AddHttpClient<IInstapaperService, InstapaperService>();
        services.AddScoped<McpHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Instapaper MCP Server starting...");

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        while (true)
        {
            var line = await Console.In.ReadLineAsync();
            if (line == null) break;

            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(line, jsonOptions);
                if (request == null) continue;

                using var scope = serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<McpHandler>();

                McpResponse response;
                try
                {
                    var result = await handler.HandleAsync(request.Method, (JsonElement?)request.Params, CancellationToken.None);
                    response = new McpResponse("2.0", request.Id, result, null);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling method {Method}", request.Method);
                    response = new McpResponse("2.0", request.Id, null, new McpError(-32603, ex.Message, null));
                }

                var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
                await Console.Out.WriteLineAsync(jsonResponse);
                await Console.Out.FlushAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fatal error in request processing loop");
            }
        }
    }
}
