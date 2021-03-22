using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Routing
{
    public static class ODataEndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapODataRouteAndSpecifyQueries(this IEndpointRouteBuilder endpoints, IEdmModel model, int maxTop)
        {
            endpoints.Select().Filter().OrderBy().Count().MaxTop(maxTop);
            return endpoints.MapODataRoute("api", "api", model);
        }


        public static IEndpointConventionBuilder MapFallBack(this IEndpointRouteBuilder endpoints)
        {
            /* The point of this is that, if the URL is to the API (as opposed to the Blazor WASM client) and we can't find the route
             * we should return a 404 not found response with a ProblemDetails. But, if the URL is to the Blazor WASM client and we
             * can't find the route, we should fall back to the client itself, which has functionality built in to show a not-found page
             */

            /*TODO: this functionality doesn't seem to be working properly with OData. I believe that OData is also specifying a fallback
             of its own, with the same priority, and they conflict with each other in the router. */

            //endpoints.Map("api/{**slug}", HandleApiFallback);
            return endpoints.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "index.html");
        }

        private static Task HandleApiFallback(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }
    }
}
