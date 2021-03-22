using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public class UsingODataWithSwaggerOptions : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            // This is to make Swagger work with OData. It's important that it comes AFTER the call to AddOData()
            // TODO: Can this be moved into the OData extension methods?
            foreach (var outputFormatter in options.OutputFormatters.OfType<OutputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
            {
                outputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
            }

            foreach (var inputFormatter in options.InputFormatters.OfType<InputFormatter>().Where(x => x.SupportedMediaTypes.Count == 0))
            {
                inputFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/prs.odatatestxx-odata"));
            }
        }
    }
}