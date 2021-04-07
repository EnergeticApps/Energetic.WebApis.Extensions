using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.AspNetCore.Authorization
{
    public class ConfigureAuthorizationOptions : IConfigureOptions<AuthorizationOptions>
    {
        private readonly SecurityOptions _apiSecurityOptions;

        public ConfigureAuthorizationOptions(IOptions<SecurityOptions> apiSecurityOptionsAccessor)
        {
            _apiSecurityOptions = apiSecurityOptionsAccessor.Value ?? throw new ArgumentNullException(nameof(apiSecurityOptionsAccessor));

            if (string.IsNullOrWhiteSpace(_apiSecurityOptions?.Jwt.Audience))
                throw new InvalidOperationException("No API information (such as audience etc.) has been passed." +
                    "Maybe it is missing from the configuration file or not registered in the dependency injection container.");
        }

        public virtual void Configure(AuthorizationOptions options)
        {
            options.AddPolicy("ApiScopeClaim", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", _apiSecurityOptions.Jwt.Audience);
            });
        }
    }
}