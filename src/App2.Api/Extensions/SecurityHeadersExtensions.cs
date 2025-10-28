using NetEscapades.AspNetCore.SecurityHeaders;

namespace App2.Api.Extensions;

public static class SecurityHeadersExtensions
{
    public static IServiceCollection AddAppSecurityHeaders(this IServiceCollection services) => services;

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IConfiguration configuration)
    {
        var policies = new HeaderPolicyCollection();
        policies.AddDefaultSecurityHeaders();

        // Remove legacy X-Frame-Options; use CSP frame-ancestors instead.
        policies.RemoveCustomHeader("X-Frame-Options");

        if (configuration.GetValue("SecurityHeaders:Csp:Enabled", false))
        {
            policies.AddContentSecurityPolicy(builder =>
            {
                builder.AddDefaultSrc().Self();
                builder.AddScriptSrc().Self().WithNonce();
                builder.AddStyleSrc().Self().UnsafeInline();
                builder.AddImgSrc().Self().Data();
                builder.AddConnectSrc().Self();

                // Critical: disallow embedding
                builder.AddFrameAncestors().None();
            });
        }

        app.UseSecurityHeaders(policies);
        return app;
    }
}
