using Instapaper.Mcp.Server.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Instapaper.Mcp.Server;

public static class InstapaperServiceCollectionExtensions
{
    public static IServiceCollection AddInstapaper(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<InstapaperOptions>()
            .Bind(configuration.GetSection(InstapaperOptions.SectionName))
            .Validate(o =>
                    !string.IsNullOrWhiteSpace(o.ConsumerKey) &&
                    !string.IsNullOrWhiteSpace(o.ConsumerSecret),
                "Instapaper ConsumerKey/ConsumerSecret are required")
            .Validate(o =>
                    !string.IsNullOrWhiteSpace(o.Username) &&
                    !string.IsNullOrWhiteSpace(o.Password),
                "Instapaper Username/Password are required")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOAuth1SignatureGenerator, OAuth1SignatureGenerator>();

        services.AddHttpClient<IInstapaperClient, InstapaperClient>(c =>
        {
            c.BaseAddress = new Uri("https://www.instapaper.com/api/1/");
        });

        return services;
    }
}