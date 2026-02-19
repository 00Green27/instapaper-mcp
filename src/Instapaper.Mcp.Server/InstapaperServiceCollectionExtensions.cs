using Instapaper.Mcp.Server.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Instapaper.Mcp.Server;

/// <summary>
/// Extension methods for configuring Instapaper services in dependency injection.
/// </summary>
public static class InstapaperServiceCollectionExtensions
{
    /// <summary>
    /// Adds Instapaper API client and related services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This method registers:
    /// - InstapaperOptions with configuration binding and validation
    /// - IOAuth1SignatureGenerator implementation
    /// - HttpClient for IInstapaperClient
    ///
    /// Configuration is validated at startup to ensure required credentials are present.
    /// </remarks>
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
            c.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}