using App2.Api.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace App2.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }

    public static IServiceCollection AddDevelopmentAuthFallback(this IServiceCollection services, IConfiguration configuration)
    {
        var headerName = configuration["Security:DevHeader:HeaderName"] ?? "X-Dev-User";

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = DevHeaderAuthenticationHandler.SchemeName;
                options.DefaultChallengeScheme = DevHeaderAuthenticationHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, DevHeaderAuthenticationHandler>(DevHeaderAuthenticationHandler.SchemeName, options =>
            {
                options.ClaimsIssuer = headerName;
            });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => true)
                .Build();
        });

        return services;
    }
}
