using Microsoft.EntityFrameworkCore;
using TuneTrail.Api.IoC.Context;

namespace TuneTrail.Api.IoC.Configs;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<TuneTrailDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });
        });

        return builder.Services;
    }
}
