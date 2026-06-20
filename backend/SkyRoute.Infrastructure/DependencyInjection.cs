using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.UseCases.SearchFlights;
using SkyRoute.Infrastructure.Persistence;
using SkyRoute.Infrastructure.Providers;
using SkyRoute.Infrastructure.Resilience;

namespace SkyRoute.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAirportRepository, InMemoryAirportRepository>();

        services.Configure<BudgetWingsOptions>(configuration.GetSection("BudgetWings"));

        services.AddSingleton(BuildProviderPipeline());

        services.AddScoped<GlobalAirProvider>();
        services.AddScoped<BudgetWingsProvider>();

        services.AddScoped<IFlightProvider>(sp =>
            new ResilientFlightProvider(sp.GetRequiredService<GlobalAirProvider>(), sp.GetRequiredService<ResiliencePipeline>()));
        services.AddScoped<IFlightProvider>(sp =>
            new ResilientFlightProvider(sp.GetRequiredService<BudgetWingsProvider>(), sp.GetRequiredService<ResiliencePipeline>()));

        services.AddScoped<SearchFlightsUseCase>();
        return services;
    }

    private static ResiliencePipeline BuildProviderPipeline() =>
        new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10)
            })
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromMilliseconds(200),
                // Only retry transient errors, not business failures like SimulateFailure
                ShouldHandle = new PredicateBuilder().Handle<HttpRequestException>().Handle<TimeoutRejectedException>()
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();
}
