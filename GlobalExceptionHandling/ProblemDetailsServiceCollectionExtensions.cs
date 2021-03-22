using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ProblemDetailsServiceCollectionExtensions
    {
        public static IServiceCollection AddProblemDetailsExceptionMapping(this IServiceCollection services, IWebHostEnvironment environment)
        {
            return services.AddProblemDetails(c =>
            {
                c.AddDatabaseProblemDetailsExceptionMapping();
                c.AddInvalidInputProblemDetailsExceptionMapping();
                c.AddSecurityProblemDetailsExceptionMapping();
                c.AddProblemDetailsExceptionMappingForWhenItsOurFault();
                c.IncludeExceptionDetails = (context, exception) => environment.IsDevelopment();
            });
        }
    }
}