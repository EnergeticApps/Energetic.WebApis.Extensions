using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GlobalizationApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestLocalization(
            this IApplicationBuilder app,
            RequestCulture defaultCulture,
            params RequestCulture[] supportedCultures)
        {
            if (app is null)
                throw new ArgumentNullException(nameof(app));

            return app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = defaultCulture ?? throw new ArgumentNullException(nameof(defaultCulture));

                if(supportedCultures.IsNotNullOrEmpty())
                {
                    options.SupportedCultures = supportedCultures.Select(rc => rc.Culture).ToList();
                    options.SupportedUICultures = supportedCultures.Select(rc => rc.UICulture).ToList();
                }
            });
        }
    }
}