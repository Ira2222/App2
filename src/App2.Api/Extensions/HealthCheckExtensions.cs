using System.Text.Json;
using App2.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace App2.Api.Extensions;

public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static IServiceCollection AddHealthChecksWithSplit(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddDbContextCheck<AppDbContext>("database", tags: new[] { "ready", "db" });

        return services;
    }

    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/healthz/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = WriteResponse
        }).AllowAnonymous();

        app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteResponse
        }).AllowAnonymous();
    }

    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                tags = entry.Value.Tags
            })
        }, SerializerOptions);

        return context.Response.WriteAsync(payload);
    }
}
