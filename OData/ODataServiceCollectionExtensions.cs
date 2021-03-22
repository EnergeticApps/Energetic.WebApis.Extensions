using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class ODataServiceCollectionExtensions
    { 
        // As of March 2021 there is no plan to make OData work with strongly typed IDs and other value objects.
      // So I'm removing OData from the template for now. TODO: Check regularly whether we should reconsider.
        public static IServiceCollection AddOData(this IServiceCollection services, bool toWorkWithSwagger)
        {
            services.AddOData();

            if(toWorkWithSwagger)
            {
                services.ConfigureOptions<UsingODataWithSwaggerOptions>(); // This line must come AFTER the call to AddOData()
            }

            return services; /* TODO: if we decide to use OData, we can configure it here. But don't forget that we have to configure 
                              * input and output formatters in the MvcOptions for it to work nicely with Swagger. Presently this is done in FormattersServiceCollectionExtensions */


        }
    }
}
