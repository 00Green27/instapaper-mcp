using Instapaper.Mcp.Server.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;

using Polly;

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
    /// - HttpClient for IInstapaperClient with resilience policies
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
                    (!string.IsNullOrWhiteSpace(o.Username) && !string.IsNullOrWhiteSpace(o.Password)) ||
                    (!string.IsNullOrWhiteSpace(o.AccessToken) && !string.IsNullOrWhiteSpace(o.AccessTokenSecret)),
                "Either Instapaper Username/Password or AccessToken/AccessTokenSecret are required")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IOAuth1SignatureGenerator, OAuth1SignatureGenerator>();

        services.AddHttpClient<IInstapaperClient, InstapaperClient>(c =>
        {
            c.BaseAddress = new Uri("https://www.instapaper.com/api/1/");
        })
        .AddStandardResilienceHandler(config =>
        {
            // Retry settings
            config.Retry.ShouldHandle = args =>
                new ValueTask<bool>((args.Outcome.Exception is HttpRequestException) ||
                                   (args.Outcome.Result is HttpResponseMessage r &&
                                    ((int)r.StatusCode >= 500 ||
                                     r.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                                     r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)));
            config.Retry.MaxRetryAttempts = 3;
            config.Retry.Delay = TimeSpan.FromSeconds(1);
            config.Retry.BackoffType = DelayBackoffType.Exponential;

            // Circuit breaker settings
            config.CircuitBreaker.ShouldHandle = args =>
                new ValueTask<bool>((args.Outcome.Exception is HttpRequestException) ||
                                   (args.Outcome.Result is HttpResponseMessage r && (int)r.StatusCode >= 500));
            config.CircuitBreaker.FailureRatio = 0.5;
            config.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            config.CircuitBreaker.MinimumThroughput = 5;
            config.CircuitBreaker.BreakDuration = TimeSpan.FromMinutes(1);
        });

        return services;
    }
}