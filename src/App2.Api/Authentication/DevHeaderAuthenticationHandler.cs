using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace App2.Api.Authentication;

public sealed class DevHeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevHeader";

    public DevHeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerName = Options.ClaimsIssuer ?? "X-Dev-User";
        if (!Context.Request.Headers.TryGetValue(headerName, out var values) || string.IsNullOrWhiteSpace(values))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity(Scheme.Name);
        identity.AddClaim(new Claim(ClaimTypes.Name, values.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, values.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Role, "Developer"));

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
