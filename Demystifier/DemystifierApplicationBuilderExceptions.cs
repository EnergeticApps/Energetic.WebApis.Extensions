using Ben.Demystifier;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Builder
{
    public static class DemystifierApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseDemystifier(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DemystifierMiddleware>();
        }
    }
}