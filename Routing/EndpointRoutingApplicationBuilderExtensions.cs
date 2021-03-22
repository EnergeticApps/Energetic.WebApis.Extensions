using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public static class EndpointRoutingApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseEndpointsWithFallbackRoute(this IApplicationBuilder app)
        {
            return app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallBack();
            });
        }
    }
}
