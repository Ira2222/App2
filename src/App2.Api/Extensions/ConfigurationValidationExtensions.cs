namespace App2.Api.Extensions;

public static class ConfigurationValidationExtensions
{
    public static IApplicationBuilder ValidateRequiredConfiguration(this IApplicationBuilder app, IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment() && !string.Equals(environment.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            var tenantId = configuration["AzureAd:TenantId"];
            var clientId = configuration["AzureAd:ClientId"];
            if (string.IsNullOrWhiteSpace(tenantId) || tenantId.Contains("TODO", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("AzureAd:TenantId must be configured for non-development environments.");
            }

            if (string.IsNullOrWhiteSpace(clientId) || clientId.Contains("TODO", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("AzureAd:ClientId must be configured for non-development environments.");
            }

            var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            if (corsOrigins.Length == 0)
            {
                throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin in non-development environments.");
            }
        }

        if (configuration.GetValue<bool>("Features:RedisOutputCache"))
        {
            var redisConnection = configuration["Redis:ConnectionString"];
            if (string.IsNullOrWhiteSpace(redisConnection))
            {
                throw new InvalidOperationException("Redis:ConnectionString must be configured when Redis output caching is enabled.");
            }
        }

        return app;
    }
}
