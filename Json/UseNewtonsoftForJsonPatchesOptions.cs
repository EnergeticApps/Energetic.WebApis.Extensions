using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public class UseNewtonsoftForJsonPatchesOptions : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            // This is for using Newtonsoft.Json for serializing JsonPatchDocuments but allowing us to use System.Text.Json for everything else
            options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
        }

        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            var builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }
    }
}