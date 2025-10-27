using App2.Api.Constants;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App2.Api.Extensions;

public static class OutputCacheRedisExtensions
{
    public static IServiceCollection AddOutputCacheWithOptionalRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var features = configuration.GetSection("Features");
        var useOutputCache = features.GetValue<bool>("OutputCaching");
        if (!useOutputCache)
        {
            return services;
        }

        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(60)));
            options.AddPolicy(AppConstants.Cache.TodosPolicy, builder => builder.Expire(TimeSpan.FromMinutes(5)).Tag(AppConstants.Cache.TodosTag));
        });

        if (features.GetValue<bool>("RedisOutputCache"))
        {
            services.AddStackExchangeRedisOutputCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"];
                options.InstanceName = configuration["Redis:InstanceName"] ?? "app2:oc:";
            });
        }

        return services;
    }
}
