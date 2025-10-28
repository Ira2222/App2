using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

                // Development fallback to localhost (safe for dev only)
                if (origins.Length == 0)
                {
                    origins = new[] { "http://localhost:5173", "https://localhost:5173" };
                }

                // Guard from PR #17: reject wildcards when credentials are enabled
                if (origins.Any(origin => origin == "*"))
                {
                    throw new InvalidOperationException("Cors:AllowedOrigins must be explicit when credentials are enabled.");
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

    public static IApplicationBuilder UseCorsVaryHeader(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            await next();

            if (context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                context.Response.Headers.AppendCommaSeparatedValues("Vary", "Origin");
            }
        });
    }
}
