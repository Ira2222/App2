using NetEscapades.AspNetCore.SecurityHeaders;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;

namespace App2.Api.Extensions;

public static class SecurityHeadersExtensions
{
    public static IServiceCollection AddAppSecurityHeaders(this IServiceCollection services) => services;

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IConfiguration configuration)
    {
        var policies = new HeaderPolicyCollection();
        policies.AddDefaultSecurityHeaders();
        policies.RemoveCustomHeader("X-Frame-Options");

        if (configuration.GetValue("SecurityHeaders:Csp:Enabled", false))
        {
            policies.AddContentSecurityPolicy(builder =>
            {
                builder.AddDefaultSrc().Self();

                var scriptSrc = builder.AddScriptSrc();
                scriptSrc.Self();

                var styleSrc = builder.AddStyleSrc();
                styleSrc.Self();
                if (configuration.GetValue("SecurityHeaders:Csp:AllowUnsafeInlineStyles", false))
                {
                    styleSrc.UnsafeInline();
                }

                var imgSrc = builder.AddImgSrc();
                imgSrc.Self();
                imgSrc.Data();

                var fontSrc = builder.AddFontSrc();
                fontSrc.Self();
                fontSrc.Data();

                var connectSrc = builder.AddConnectSrc();
                connectSrc.Self();

                builder.AddFrameAncestors().None();
            });
        }

        return app.UseSecurityHeaders(policies);
    }
}
