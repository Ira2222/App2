using System;
using App2.Api.Options;
using App2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace App2.Api.Extensions;

public static class DataExtensions
{
    public static IServiceCollection AddAppData(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var provider = databaseOptions.Provider ?? "Sqlite";

            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                var connection = databaseOptions.ConnectionStrings.Postgres;
                if (string.IsNullOrWhiteSpace(connection) && environment.IsDevelopment())
                {
                    connection = "Host=localhost;Database=app2;Username=postgres;Password=postgres";
                }

                if (string.IsNullOrWhiteSpace(connection))
                {
                    throw new InvalidOperationException("Data:ConnectionStrings:Postgres must be configured when using the Postgres provider.");
                }
                options.UseNpgsql(connection, npgsql =>
                {
                    npgsql.EnableRetryOnFailure();
                });
            }
            else
            {
                var path = databaseOptions.ConnectionStrings.Sqlite;
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = "Data Source=app2.db";
                }
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
