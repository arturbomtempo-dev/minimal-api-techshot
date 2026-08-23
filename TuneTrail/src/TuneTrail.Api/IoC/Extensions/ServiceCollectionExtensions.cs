using TuneTrail.Api.Aggregate;
using TuneTrail.Api.Contract;

namespace TuneTrail.Api.IoC.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IMusicAggregate, MusicAggregate>();

        return services;
    }
}
