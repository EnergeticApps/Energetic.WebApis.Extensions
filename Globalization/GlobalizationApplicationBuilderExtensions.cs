using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GlobalizationApplicationBuilderExtensions
    {
        private static RequestCulture BritishEnglish = new RequestCulture("en-GB");

        public static IApplicationBuilder UseRequestLocalization(
            this IApplicationBuilder app,
            RequestCulture? defaultCulture = null,
            params RequestCulture[] supportedCultures)
        {
            return app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = defaultCulture ?? BritishEnglish;
                options.SupportedCultures = supportedCultures.Select(rc => rc.Culture).ToList();
                options.SupportedUICultures = supportedCultures.Select(rc => rc.UICulture).ToList();
            });
        }
    }
}