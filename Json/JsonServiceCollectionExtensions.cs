using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JsonServiceCollectionExtensions
    {
        public static IServiceCollection AddNewtonsoftJsonForJsonPatchDocumentsAndSystemTextJsonForEverythingElse(this IServiceCollection services)
        {
            return services.ConfigureOptions<UseNewtonsoftForJsonPatchesOptions>();
        }
    }
}
