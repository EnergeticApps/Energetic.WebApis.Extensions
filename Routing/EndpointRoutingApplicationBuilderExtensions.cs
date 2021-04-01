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
                endpoints.MapControllers()
                ;//.RequireAuthorization("ApiScope"); // TODO: is this the best place for this line of code? Shouldn't it be moved somewhere to do with authorization?
                endpoints.MapFallBack();
            });
        }
    }
}
