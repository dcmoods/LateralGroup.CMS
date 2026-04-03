using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace LateralGroup.API.Authentication;

public sealed class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly BasicAuthOptions _authOptions;

    public BasicAuthenticationHandler(
        IOptions<BasicAuthOptions> authOptions,
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _authOptions = authOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!AuthenticationHeaderValue.TryParse(authorizationHeaderValues, out var headerValue) ||
            !string.Equals(headerValue.Scheme, AuthConstants.Scheme, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header."));
        }

        string username;
        string password;

        try
        {
            var credentialBytes = Convert.FromBase64String(headerValue.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes);
            var separatorIndex = credentials.IndexOf(':');
            if (separatorIndex <= 0)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid basic authentication credentials."));
            }

            username = credentials[..separatorIndex];
            password = credentials[(separatorIndex + 1)..];
        }
        catch (FormatException)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid base64 payload."));
        }

        var matchingUser = FindMatchingUser(username, password);
        if (matchingUser is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid username or password."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, matchingUser.Username)
        };

        claims.AddRange(matchingUser.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = "Basic realm=\"LateralGroup.CMS\"";
        return base.HandleChallengeAsync(properties);
    }

    private BasicAuthUserOptions? FindMatchingUser(string username, string password)
    {
        foreach (var candidate in EnumerateUsers())
        {
            if (string.Equals(candidate.Username, username, StringComparison.Ordinal) &&
                string.Equals(candidate.Password, password, StringComparison.Ordinal))
            {
                return candidate;
            }
        }

        return null;
    }

    private IEnumerable<BasicAuthUserOptions> EnumerateUsers()
    {
        yield return _authOptions.Cms;
        yield return _authOptions.Consumer;
        yield return _authOptions.Admin;
    }
}
