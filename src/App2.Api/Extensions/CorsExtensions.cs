namespace App2.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsAllowList(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowedOrigins", policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                // In production, explicitly require CORS origins to be configured
                if (environment.IsProduction() && origins.Length == 0)
                {
                    throw new InvalidOperationException(
                        "CORS AllowedOrigins must be explicitly configured in Production environment. " +
                        "Set 'Cors:AllowedOrigins' in appsettings.Production.json with your frontend URL(s).");
                }

                // Development fallback to localhost (safe for dev only)
                if (origins.Length == 0)
                {
                    origins = new[] { "http://localhost:5173", "https://localhost:5173" };
                }

                var methods = configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "PATCH" };
                var headers = configuration.GetSection("Cors:AllowedHeaders").Get<string[]>() ?? new[] { "Content-Type", "Authorization" };

                policy.WithOrigins(origins)
                      .WithMethods(methods)
                      .WithHeaders(headers)
                      .AllowCredentials();
            });
        });

        return services;
    }
}
