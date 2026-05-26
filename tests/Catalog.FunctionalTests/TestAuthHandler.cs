using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eShop.Catalog.FunctionalTests;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string AdminToken = "admin";
    public const string UserToken = "user";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!AuthenticationHeaderValue.TryParse(authorizationHeader!, out var headerValue) ||
            !string.Equals(headerValue.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = headerValue.Parameter.Trim().ToLowerInvariant() switch
        {
            AdminToken => new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "admin-user"),
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Role, "admin")
                ],
                SchemeName),
            UserToken => new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "regular-user"),
                    new Claim(ClaimTypes.Name, "user"),
                    new Claim(ClaimTypes.Role, "user")
                ],
                SchemeName),
            _ => null
        };

        if (identity is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Unknown test token."));
        }

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}