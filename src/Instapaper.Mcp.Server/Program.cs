using Instapaper.Mcp.Server;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ModelContextProtocol.Protocol;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Information;
});

builder.Services
    .AddInstapaper(builder.Configuration)
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "Instapaper Server",
            Version = "1.0.0",
            Title = "MCP Instapaper Server",
            Description = "A comprehensive MCP server for Instapaper integration",
            Icons = [
                new Icon
                {
                    Source = "https://d2kfnvwohu0503.cloudfront.net/img/icon.svg",
                    MimeType = "image/svg+xml",
                    Sizes = ["any"],
                    Theme = "light"
                },
                new Icon
                {
                    Source = "https://d2kfnvwohu0503.cloudfront.net/img/favicon.png",
                    MimeType = "image/png",
                    Sizes = ["32x32"]
                }
            ]
        };
    })
    .WithStdioServerTransport()
    .WithTools<InstapaperBookmarkTools>()
    .WithTools<InstapaperFolderTools>()
    .WithResources<InstapaperResources>();

await builder.Build().RunAsync();