using Microsoft.Extensions.DependencyInjection;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.UseCases.SearchFlights;
using SkyRoute.Infrastructure.Persistence;
using SkyRoute.Infrastructure.Providers;

namespace SkyRoute.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAirportRepository, InMemoryAirportRepository>();
        services.AddScoped<IFlightProvider, GlobalAirProvider>();
        services.AddScoped<SearchFlightsUseCase>();
        return services;
    }
}
