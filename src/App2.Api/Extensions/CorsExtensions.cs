namespace App2.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsAllowList(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowedOrigins", policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                if (origins.Length == 0)
                {
                    origins = new[] { "http://localhost:5173" };
                }

                if (origins.Any(origin => origin == "*"))
                {
                    throw new InvalidOperationException("Cors:AllowedOrigins must be explicit when credentials are enabled.");
                }

                var methods = configuration.GetSection("Cors:AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST" };
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
