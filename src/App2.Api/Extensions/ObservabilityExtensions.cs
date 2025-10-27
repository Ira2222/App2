using OpenTelemetry.Exporter;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace App2.Api.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddAppObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["OpenTelemetry:Endpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return services;
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("App2.Api"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(endpoint);
                    exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(endpoint);
                    exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                }));

        return services;
    }
}
