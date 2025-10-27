using System.Threading.RateLimiting;
using App2.Api.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace App2.Api.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRequestRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue<int?>("RateLimiting:Fixed:PermitLimit") ?? 100;
        var windowSeconds = configuration.GetValue<int?>("RateLimiting:Fixed:WindowSeconds") ?? 60;
        var queueLimit = configuration.GetValue<int?>("RateLimiting:Fixed:QueueLimit") ?? 5;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return ValueTask.CompletedTask;
            };

            options.AddFixedWindowLimiter(AppConstants.RateLimiting.FixedPolicy, limiterOptions =>
            {
                limiterOptions.AutoReplenishment = true;
                limiterOptions.PermitLimit = permitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(windowSeconds);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = queueLimit;
            });
        });

        return services;
    }
}
