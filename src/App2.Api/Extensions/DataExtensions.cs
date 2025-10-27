using App2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace App2.Api.Extensions;

public static class DataExtensions
{
    public static IServiceCollection AddAppData(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var provider = configuration["Data:Provider"] ?? "Sqlite";
            var connectionStrings = configuration.GetSection("Data:ConnectionStrings");

            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                var connection = connectionStrings["Postgres"] ?? "Host=localhost;Database=app2;Username=postgres;Password=postgres";
                options.UseNpgsql(connection, npgsql =>
                {
                    npgsql.EnableRetryOnFailure();
                });
            }
            else
            {
                var path = configuration["Data:ConnectionStrings:Sqlite"] ?? "Data Source=app2.db";
                options.UseSqlite(path);
            }

            if (environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
}
