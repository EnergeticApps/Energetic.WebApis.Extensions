using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Microsoft.Extensions.DependencyInjection.Authorization
{
    public class ConfigureJwtBearerOptions : IConfigureOptions<JwtBearerOptions>
    {
        private readonly SecurityOptions _options;

        public ConfigureJwtBearerOptions(IOptions<SecurityOptions> optionsAccessor)
        {
            _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));

            if (string.IsNullOrWhiteSpace(_options?.Jwt.Authority))
                throw new InvalidOperationException("No API information (such as authority etc.) has been passed." +
                    "Maybe it is missing from the configuration file or not registered in the dependency injection container.");
        }

        public void Configure(JwtBearerOptions options)
        {
            options.RequireHttpsMetadata = false;
            options.Authority = _options.Jwt.Authority;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
        }
    }
}