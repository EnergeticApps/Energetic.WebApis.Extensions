using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.AspNetCore.Authorization
{
    public class ConfigureAuthorizationOptions : IConfigureOptions<AuthorizationOptions>
    {
        private readonly SecurityOptions _apiSecurityOptions;

        public ConfigureAuthorizationOptions(IOptions<SecurityOptions> apiSecurityOptionsAccessor, IWebHostEnvironment environment)
        {
            bool isDevelopmentEnvironment = environment.IsDevelopment();
            _apiSecurityOptions = apiSecurityOptionsAccessor.Value ?? throw new ArgumentNullException(nameof(apiSecurityOptionsAccessor));

            if(_apiSecurityOptions.Jwt is null && !isDevelopmentEnvironment)
            {
                throw new InvalidOperationException("No JWT information (such as Authority etc.) has been passed. Maybe it is missing from the configuration file or not registered in the dependency injection container.");
            }
        }

        public virtual void Configure(AuthorizationOptions options)
        {
            options.AddPolicy("ApiScopeClaim", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("scope", _apiSecurityOptions.Jwt?.Audience);
            });
        }
    }
}