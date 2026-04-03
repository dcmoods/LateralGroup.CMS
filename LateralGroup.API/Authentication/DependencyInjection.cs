using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace LateralGroup.API.Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddBasicAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection(BasicAuthOptions.SectionName);
        var authOptions = authSection.Get<BasicAuthOptions>() ?? new BasicAuthOptions();
        Validate(authOptions);

        services.AddSingleton(authOptions);

        services
            .AddAuthentication(AuthConstants.Scheme)
            .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(
                AuthConstants.Scheme,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(AuthConstants.Scheme)
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(AuthConstants.CmsPolicy, policy =>
            {
                policy.AddAuthenticationSchemes(AuthConstants.Scheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AuthConstants.CmsRole);
            });

            options.AddPolicy(AuthConstants.ConsumerPolicy, policy =>
            {
                policy.AddAuthenticationSchemes(AuthConstants.Scheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AuthConstants.ConsumerRole, AuthConstants.AdminRole);
            });

            options.AddPolicy(AuthConstants.AdminPolicy, policy =>
            {
                policy.AddAuthenticationSchemes(AuthConstants.Scheme);
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AuthConstants.AdminRole);
            });
        });

        return services;
    }

    private static void Validate(BasicAuthOptions options)
    {
        ValidateUser(options.Cms, nameof(options.Cms));
        ValidateUser(options.Consumer, nameof(options.Consumer));
        ValidateUser(options.Admin, nameof(options.Admin));
    }

    private static void ValidateUser(BasicAuthUserOptions user, string name)
    {
        if (string.IsNullOrWhiteSpace(user.Username))
        {
            throw new InvalidOperationException($"Basic auth user '{name}' must define a username.");
        }

        if (user.Username.Length < 10 || user.Username.Length > 20)
        {
            throw new InvalidOperationException($"Basic auth user '{name}' must have a username length between 10 and 20 characters.");
        }

        if (string.IsNullOrWhiteSpace(user.Password))
        {
            throw new InvalidOperationException($"Basic auth user '{name}' must define a password.");
        }

        if (!Guid.TryParse(user.Password, out _))
        {
            throw new InvalidOperationException($"Basic auth user '{name}' must use a GUID password.");
        }

        if (user.Roles.Length == 0)
        {
            throw new InvalidOperationException($"Basic auth user '{name}' must define at least one role.");
        }
    }
}
